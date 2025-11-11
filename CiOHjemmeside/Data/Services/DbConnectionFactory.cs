using Npgsql;
using System.Data;

namespace CiOHjemmeside.Data.Services
{
    /// <summary>
    /// Implementering af IDbConnectionFactory, der bruger den singleton NpgsqlDataSource
    /// registreret i Program.cs til effektivt at håndtere connection pooling.
    /// </summary>
    public class DbConnectionFactory : IDbConnectionFactory
    {
        private readonly NpgsqlDataSource _dataSource;

        public DbConnectionFactory(NpgsqlDataSource dataSource)
        {
            _dataSource = dataSource;
        }

        public async Task<IDbConnection> CreateConnectionAsync()
        {
            // Henter en forbindelse fra den managede pool
            var connection = _dataSource.CreateConnection();
            await connection.OpenAsync();
            return connection;
        }
    }
}