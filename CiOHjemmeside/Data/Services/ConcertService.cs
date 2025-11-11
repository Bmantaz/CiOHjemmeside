using CiOHjemmeside.Data.Models;
using Dapper;

namespace CiOHjemmeside.Data.Services
{
    /// <summary>
    /// Håndterer dataadgang for Concerts ved hjælp af Dapper.
    /// </summary>
    public class ConcertService : IConcertService
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public ConcertService(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<Concert>> GetUpcomingConcertsAsync()
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();

            // Vi bruger store bogstaver for tabel- og kolonnenavne for at matche
            // din SQL-definition, da PostgreSQL er case-sensitiv.
            var sql = @"
                SELECT 
                    ""Id"", 
                    ""VenueName"", 
                    ""City"", 
                    ""Country"", 
                    ""EventDate"", 
                    ""TicketLink"", 
                    ""IsSoldOut""
                FROM ""Concerts""
                WHERE ""EventDate"" >= @CurrentDate 
                ORDER BY ""EventDate"" ASC";

            // Vi bruger UTC for at sikre konsistens på tværs af tidszoner
            return await connection.QueryAsync<Concert>(sql, new { CurrentDate = DateTime.UtcNow });
        }
    }
}