using CiOHjemmeside.Data.Models;
using Dapper;

namespace CiOHjemmeside.Data.Services
{
    public class UserService : IUserService
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public UserService(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            // Rettet til lowercase
            var sql = @"SELECT * FROM users ORDER BY username ASC";
            return await connection.QueryAsync<User>(sql);
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            // Rettet til lowercase
            var sql = @"SELECT * FROM users WHERE id = @Id";
            return await connection.QuerySingleOrDefaultAsync<User>(sql, new { Id = id });
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            // Rettet til lowercase
            var sql = @"SELECT * FROM users WHERE username = @Username";
            return await connection.QuerySingleOrDefaultAsync<User>(sql, new { Username = username });
        }

        public async Task<int> AddAsync(User user)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            // Rettet til lowercase
            var sql = @"
                INSERT INTO users (username, passwordhash, role)
                VALUES (@Username, @PasswordHash, @Role)
                RETURNING id";

            return await connection.QuerySingleAsync<int>(sql, user);
        }

        public async Task<bool> UpdateAsync(User user)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            // Rettet til lowercase
            var sql = @"
                UPDATE users SET
                    username = @Username,
                    passwordhash = @PasswordHash,
                    role = @Role
                WHERE id = @Id";

            var affectedRows = await connection.ExecuteAsync(sql, user);
            return affectedRows > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            // Rettet til lowercase
            var sql = @"DELETE FROM users WHERE id = @Id";
            var affectedRows = await connection.ExecuteAsync(sql, new { Id = id });
            return affectedRows > 0;
        }
    }
}