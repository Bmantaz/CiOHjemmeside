using CiOHjemmeside.Data.Models;
using Dapper;

namespace CiOHjemmeside.Data.Services
{
    public class MerchandiseService : IMerchandiseService
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public MerchandiseService(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<MerchandiseItem>> GetAllAsync()
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();

            // SQL'en bruger de case-sensitive navne fra din skema-definition.
            // Modellen MerchandiseItem mappes til "Merchandise"-tabellen via [Table]-attributten.
            var sql = @"SELECT * FROM ""Merchandise"" ORDER BY ""ItemName"" ASC";
            return await connection.QueryAsync<MerchandiseItem>(sql);
        }

        public async Task<MerchandiseItem?> GetByIdAsync(int id)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"SELECT * FROM ""Merchandise"" WHERE ""Id"" = @Id";
            return await connection.QuerySingleOrDefaultAsync<MerchandiseItem>(sql, new { Id = id });
        }

        public async Task<int> AddAsync(MerchandiseItem item)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                INSERT INTO ""Merchandise"" (""ItemName"", ""Description"", ""StockQuantity"", ""Price"")
                VALUES (@ItemName, @Description, @StockQuantity, @Price)
                RETURNING ""Id"""; // RETURNING "Id" henter det auto-genererede ID tilbage.

            return await connection.QuerySingleAsync<int>(sql, item);
        }

        public async Task<bool> UpdateAsync(MerchandiseItem item)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                UPDATE ""Merchandise"" SET
                    ""ItemName"" = @ItemName,
                    ""Description"" = @Description,
                    ""StockQuantity"" = @StockQuantity,
                    ""Price"" = @Price
                WHERE ""Id"" = @Id";

            var affectedRows = await connection.ExecuteAsync(sql, item);
            return affectedRows > 0; // Returnerer true hvis 1 (eller flere) rækker blev opdateret
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"DELETE FROM ""Merchandise"" WHERE ""Id"" = @Id";
            var affectedRows = await connection.ExecuteAsync(sql, new { Id = id });
            return affectedRows > 0;
        }
    }
}