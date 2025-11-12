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
            // Rettet til lowercase
            var sql = @"SELECT * FROM merchandise ORDER BY itemname ASC";
            return await connection.QueryAsync<MerchandiseItem>(sql);
        }

        public async Task<MerchandiseItem?> GetByIdAsync(int id)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            // Rettet til lowercase
            var sql = @"SELECT * FROM merchandise WHERE id = @Id";
            return await connection.QuerySingleOrDefaultAsync<MerchandiseItem>(sql, new { Id = id });
        }

        public async Task<int> AddAsync(MerchandiseItem item)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            // Rettet til lowercase
            var sql = @"
                INSERT INTO merchandise (itemname, description, stockquantity, price)
                VALUES (@ItemName, @Description, @StockQuantity, @Price)
                RETURNING id";

            return await connection.QuerySingleAsync<int>(sql, item);
        }

        public async Task<bool> UpdateAsync(MerchandiseItem item)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            // Rettet til lowercase
            var sql = @"
                UPDATE merchandise SET
                    itemname = @ItemName,
                    description = @Description,
                    stockquantity = @StockQuantity,
                    price = @Price
                WHERE id = @Id";

            var affectedRows = await connection.ExecuteAsync(sql, item);
            return affectedRows > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            // Rettet til lowercase
            var sql = @"DELETE FROM merchandise WHERE id = @Id";
            var affectedRows = await connection.ExecuteAsync(sql, new { Id = id });
            return affectedRows > 0;
        }
    }
}