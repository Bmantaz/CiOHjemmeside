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

        public async Task RecordSaleAsync(int soldByUserId, IEnumerable<SaleItemInput> items, DateTime? soldAtUtc = null)
        {
            var normalizedItems = items
                .Where(i => i.Quantity > 0)
                .Select(i => new SaleItemInput
                {
                    ProductGroupName = i.ProductGroupName,
                    VariantName = i.VariantName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                })
                .ToList();

            if (!normalizedItems.Any())
            {
                return;
            }

            using var connection = await _connectionFactory.CreateConnectionAsync();
            await EnsureSchemaAsync(connection);

            using var transaction = connection.BeginTransaction();

            try
            {
                var soldAt = soldAtUtc ?? DateTime.UtcNow;
                var totalAmount = normalizedItems.Sum(i => i.Quantity * i.UnitPrice);

                var saleId = await connection.QuerySingleAsync<int>(
                    @"INSERT INTO sales (soldat, soldbyuserid, totalamount)
                      VALUES (@SoldAt, @SoldByUserId, @TotalAmount)
                      RETURNING id",
                    new
                    {
                        SoldAt = soldAt,
                        SoldByUserId = soldByUserId,
                        TotalAmount = totalAmount
                    },
                    transaction);

                const string saleItemSql = @"
                    INSERT INTO saleitems
                        (saleid, productgroupname, variantname, quantity, unitprice, lineamount)
                    VALUES
                        (@SaleId, @ProductGroupName, @VariantName, @Quantity, @UnitPrice, @LineAmount)";

                foreach (var item in normalizedItems)
                {
                    await connection.ExecuteAsync(
                        saleItemSql,
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
