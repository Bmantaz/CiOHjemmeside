using CiOHjemmeside.Data.Models;
using Dapper;

namespace CiOHjemmeside.Data.Services
{
    public class CalendarEventService : ICalendarEventService
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public CalendarEventService(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<CalendarEvent>> GetEventsForPeriodAsync(DateTime startTime, DateTime endTime)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();

            // Denne SQL henter alle events, hvis 'StartTime' falder indenfor
            // den angivne periode. PostgreSQL er case-sensitiv.
            var sql = @"
                SELECT * FROM ""CalendarEvents""
                WHERE ""StartTime"" >= @StartTime AND ""StartTime"" <= @EndTime
                ORDER BY ""StartTime"" ASC";

            return await connection.QueryAsync<CalendarEvent>(sql, new { StartTime = startTime, EndTime = endTime });
        }

        public async Task<CalendarEvent?> GetByIdAsync(int id)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"SELECT * FROM ""CalendarEvents"" WHERE ""Id"" = @Id";
            return await connection.QuerySingleOrDefaultAsync<CalendarEvent>(sql, new { Id = id });
        }

        public async Task<int> AddAsync(CalendarEvent calendarEvent)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                INSERT INTO ""CalendarEvents"" 
                    (""Title"", ""EventType"", ""StartTime"", ""EndTime"", ""Notes"", ""CreatedByUserId"")
                VALUES 
                    (@Title, @EventType, @StartTime, @EndTime, @Notes, @CreatedByUserId)
                RETURNING ""Id""";

            return await connection.QuerySingleAsync<int>(sql, calendarEvent);
        }

        public async Task<bool> UpdateAsync(CalendarEvent calendarEvent)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                UPDATE ""CalendarEvents"" SET
                    ""Title"" = @Title,
                    ""EventType"" = @EventType,
                    ""StartTime"" = @StartTime,
                    ""EndTime"" = @EndTime,
                    ""Notes"" = @Notes,
                    ""CreatedByUserId"" = @CreatedByUserId
                WHERE ""Id"" = @Id";

            var affectedRows = await connection.ExecuteAsync(sql, calendarEvent);
            return affectedRows > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"DELETE FROM ""CalendarEvents"" WHERE ""Id"" = @Id";
            var affectedRows = await connection.ExecuteAsync(sql, new { Id = id });
            return affectedRows > 0;
        }
    }
}