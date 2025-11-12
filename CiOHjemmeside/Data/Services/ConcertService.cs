using CiOHjemmeside.Data.Models;
using Dapper;

namespace CiOHjemmeside.Data.Services
{
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

            // Rettet: Alle tabel- og kolonnenavne er nu i lowercase og uden ""
            // Dapper vil automatisk mappe 'venuename' til 'VenueName' osv.
            var sql = @"
                SELECT 
                    id, 
                    venuename, 
                    city, 
                    country, 
                    eventdate, 
                    ticketlink, 
                    issoldout
                FROM concerts
                WHERE eventdate >= @CurrentDate 
                ORDER BY eventdate ASC";

            return await connection.QueryAsync<Concert>(sql, new { CurrentDate = DateTime.UtcNow });
        }
    }
}