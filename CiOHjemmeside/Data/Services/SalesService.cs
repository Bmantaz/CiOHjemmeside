using CiOHjemmeside.Data.Models;
using Dapper;
using System.Data;

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
            await EnsureSchemaAsync(connection);
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

        public async Task<SalesStatisticsResult> GetStatisticsForDateAsync(DateTime date)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            await EnsureSchemaAsync(connection);

            var dayStart = date.Date;
            var dayEnd = dayStart.AddDays(1);

            var rows = (await connection.QueryAsync<SalesStatisticRow>(
                @"SELECT
                      si.productgroupname AS ProductGroupName,
                      si.variantname AS VariantName,
                      SUM(si.quantity)::int AS TotalSold,
                      COALESCE(SUM(si.lineamount), 0)::numeric AS Revenue
                  FROM sales s
                  INNER JOIN saleitems si ON si.saleid = s.id
                  WHERE s.soldat >= @DayStart AND s.soldat < @DayEnd
                  GROUP BY si.productgroupname, si.variantname
                  ORDER BY si.productgroupname, si.variantname",
                new { DayStart = dayStart, DayEnd = dayEnd })).ToList();

            var summary = await connection.QuerySingleAsync<SalesSummaryRow>(
                @"SELECT
                      COALESCE(SUM(si.quantity), 0)::int AS TotalItemsSold,
                      COALESCE(SUM(si.lineamount), 0)::numeric AS TotalRevenue
                  FROM sales s
                  INNER JOIN saleitems si ON si.saleid = s.id
                  WHERE s.soldat >= @DayStart AND s.soldat < @DayEnd",
                new { DayStart = dayStart, DayEnd = dayEnd });

            return new SalesStatisticsResult
            {
                Date = dayStart,
                Rows = rows,
                TotalItemsSold = summary.TotalItemsSold,
                TotalRevenue = summary.TotalRevenue
            };
        }

        private static Task EnsureSchemaAsync(IDbConnection connection)
        {
            const string sql = @"
                CREATE TABLE IF NOT EXISTS sales (
                    id SERIAL PRIMARY KEY,
                    soldat TIMESTAMPTZ NOT NULL,
                    soldbyuserid INT NOT NULL,
                    totalamount NUMERIC(10,2) NOT NULL
                );
                CREATE TABLE IF NOT EXISTS saleitems (
                    id SERIAL PRIMARY KEY,
                    saleid INT NOT NULL REFERENCES sales(id) ON DELETE CASCADE,
                    productgroupname TEXT NOT NULL,
                    variantname TEXT NOT NULL,
                    quantity INT NOT NULL,
                    unitprice NUMERIC(10,2) NOT NULL,
                    lineamount NUMERIC(10,2) NOT NULL
                );
                CREATE INDEX IF NOT EXISTS idx_sales_soldat ON sales (soldat);
                CREATE INDEX IF NOT EXISTS idx_saleitems_saleid ON saleitems (saleid);
            ";
            return connection.ExecuteAsync(sql);
        }

        private class SalesSummaryRow
        {
            public int TotalItemsSold { get; set; }
            public decimal TotalRevenue { get; set; }
        }
    }
}