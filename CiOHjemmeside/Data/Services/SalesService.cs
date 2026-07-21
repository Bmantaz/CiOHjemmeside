using CiOHjemmeside.Data.Models;
using Dapper;

namespace CiOHjemmeside.Data.Services
{
    public class SalesService : ISalesService
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public SalesService(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<List<ProductGroup>> GetAllProductGroupsWithVariantsAsync()
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();

            const string sql = @"
                SELECT 
                    g.id, g.groupname, g.imageurl,
                    v.id, v.productgroupid, v.variantname, v.stockquantity, v.price
                FROM productgroups g
                LEFT JOIN productvariants v ON g.id = v.productgroupid
                ORDER BY g.groupname, v.variantname";

            var groupDict = new Dictionary<int, ProductGroup>();

            await connection.QueryAsync<ProductGroup, ProductVariant, ProductGroup>(
                sql,
                (group, variant) =>
                {
                    if (!groupDict.TryGetValue(group.Id, out var currentGroup))
                    {
                        currentGroup = group;
                        currentGroup.Variants = new List<ProductVariant>();
                        groupDict.Add(group.Id, currentGroup);
                    }

                    if (variant != null && variant.Id > 0)
                    {
                        currentGroup.Variants.Add(variant);
                    }

                    return currentGroup;
                },
                splitOn: "id");

            return groupDict.Values.ToList();
        }

        public async Task RecordSaleAsync(int soldByUserId, IEnumerable<SaleItemInput> items, DateTime? soldAtUtc = null)
        {
            var normalizedItems = items.Where(i => i.Quantity > 0).ToList();
            if (!normalizedItems.Any()) return;

            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                var soldAt = soldAtUtc ?? DateTime.UtcNow;
                var totalAmount = normalizedItems.Sum(i => i.Quantity * i.UnitPrice);

                // 1. Opret salgs-hovedpost
                var saleId = await connection.QuerySingleAsync<int>(
                    @"INSERT INTO sales (soldat, soldbyuserid, totalamount)
                      VALUES (@SoldAt, @SoldByUserId, @TotalAmount)
                      RETURNING id",
                    new { SoldAt = soldAt, SoldByUserId = soldByUserId, TotalAmount = totalAmount },
                    transaction);

                foreach (var item in normalizedItems)
                {
                    // 2. Indsæt salgslinje
                    await connection.ExecuteAsync(
                        @"INSERT INTO saleitems (saleid, productgroupname, variantname, quantity, unitprice, lineamount)
                          VALUES (@SaleId, @ProductGroupName, @VariantName, @Quantity, @UnitPrice, @LineAmount)",
                        new
                        {
                            SaleId = saleId,
                            item.ProductGroupName,
                            item.VariantName,
                            item.Quantity,
                            item.UnitPrice,
                            LineAmount = item.Quantity * item.UnitPrice
                        },
                        transaction);

                    // 3. Opdater det faktiske lager i databasen
                    await connection.ExecuteAsync(
                        @"UPDATE productvariants 
                          SET stockquantity = stockquantity - @Quantity 
                          WHERE variantname = @VariantName 
                          AND productgroupid = (SELECT id FROM productgroups WHERE groupname = @ProductGroupName LIMIT 1)",
                        new { item.Quantity, item.VariantName, item.ProductGroupName },
                        transaction);
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public Task<SalesStatisticsResult> GetStatisticsForDateAsync(DateTime date)
        {
            return GetStatisticsForRangeInternalAsync(date.Date, date.Date, date.Date.AddDays(1));
        }

        public Task<SalesStatisticsResult> GetStatisticsForRangeAsync(DateTime from, DateTime to)
        {
            return GetStatisticsForRangeInternalAsync(from.Date, from.Date, to.Date.AddDays(1));
        }

        private async Task<SalesStatisticsResult> GetStatisticsForRangeInternalAsync(DateTime resultDate, DateTime rangeStart, DateTime rangeEndExclusive)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();

            var rows = (await connection.QueryAsync<SalesStatisticRow>(
                @"SELECT
                      si.productgroupname AS ProductGroupName,
                      si.variantname AS VariantName,
                      SUM(si.quantity)::int AS TotalSold,
                      COALESCE(SUM(si.lineamount), 0)::numeric AS Revenue
                  FROM sales s
                  INNER JOIN saleitems si ON si.saleid = s.id
                  WHERE s.soldat >= @RangeStart AND s.soldat < @RangeEnd
                  GROUP BY si.productgroupname, si.variantname
                  ORDER BY si.productgroupname, si.variantname",
                new { RangeStart = rangeStart, RangeEnd = rangeEndExclusive })).ToList();

            var summary = await connection.QuerySingleAsync<SalesSummaryRow>(
                @"SELECT
                      COALESCE(SUM(si.quantity), 0)::int AS TotalItemsSold,
                      COALESCE(SUM(si.lineamount), 0)::numeric AS TotalRevenue
                  FROM sales s
                  INNER JOIN saleitems si ON si.saleid = s.id
                  WHERE s.soldat >= @RangeStart AND s.soldat < @RangeEnd",
                new { RangeStart = rangeStart, RangeEnd = rangeEndExclusive });

            return new SalesStatisticsResult
            {
                Date = resultDate,
                Rows = rows,
                TotalItemsSold = summary.TotalItemsSold,
                TotalRevenue = summary.TotalRevenue
            };
        }

        public async Task<List<DailySalesSummary>> GetDailySalesSummaryAsync(DateTime from, DateTime to)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();

            var rangeStart = from.Date;
            var rangeEnd = to.Date.AddDays(1);

            var rows = (await connection.QueryAsync<DailySalesSummary>(
                @"SELECT
                      date_trunc('day', s.soldat)::date AS Date,
                      COALESCE(SUM(si.quantity), 0)::int AS TotalItemsSold,
                      COALESCE(SUM(si.lineamount), 0)::numeric AS TotalRevenue
                  FROM sales s
                  INNER JOIN saleitems si ON si.saleid = s.id
                  WHERE s.soldat >= @RangeStart AND s.soldat < @RangeEnd
                  GROUP BY date_trunc('day', s.soldat)
                  ORDER BY date_trunc('day', s.soldat)",
                new { RangeStart = rangeStart, RangeEnd = rangeEnd })).ToList();

            return rows;
        }

        private class SalesSummaryRow
        {
            public int TotalItemsSold { get; set; }
            public decimal TotalRevenue { get; set; }
        }
    }
}