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
                    otherbands,
                    ticketlink, 
                    facebookeventlink,
                    issoldout
                FROM concerts
                WHERE eventdate >= @CurrentDate 
                ORDER BY eventdate ASC";

            return await connection.QueryAsync<Concert>(sql, new { CurrentDate = DateTime.UtcNow });
        }

        public async Task<int> AddAsync(Concert concert)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();

            var ensureColumnsSql = @"
                ALTER TABLE concerts ADD COLUMN IF NOT EXISTS otherbands TEXT;
                ALTER TABLE concerts ADD COLUMN IF NOT EXISTS facebookeventlink TEXT;
            ";
            await connection.ExecuteAsync(ensureColumnsSql);

            var sql = @"
                INSERT INTO concerts (venuename, city, country, eventdate, otherbands, ticketlink, facebookeventlink, issoldout)
                VALUES (@VenueName, @City, @Country, @EventDate, @OtherBands, @TicketLink, @FacebookEventLink, @IsSoldOut)
                RETURNING id";

            return await connection.QuerySingleAsync<int>(sql, concert);
        }
    }
}