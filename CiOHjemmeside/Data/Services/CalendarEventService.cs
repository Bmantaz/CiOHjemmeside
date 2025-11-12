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
            // Rettet til lowercase
            var sql = @"
                SELECT * FROM calendarevents
                WHERE starttime >= @StartTime AND starttime <= @EndTime
                ORDER BY starttime ASC";

            return await connection.QueryAsync<CalendarEvent>(sql, new { StartTime = startTime, EndTime = endTime });
        }

        public async Task<CalendarEvent?> GetByIdAsync(int id)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            // Rettet til lowercase
            var sql = @"SELECT * FROM calendarevents WHERE id = @Id";
            return await connection.QuerySingleOrDefaultAsync<CalendarEvent>(sql, new { Id = id });
        }

        public async Task<int> AddAsync(CalendarEvent calendarEvent)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            // Rettet til lowercase
            var sql = @"
                INSERT INTO calendarevents 
                    (title, eventtype, starttime, endtime, notes, createdbyuserid)
                VALUES 
                    (@Title, @EventType, @StartTime, @EndTime, @Notes, @CreatedByUserId)
                RETURNING id";

            return await connection.QuerySingleAsync<int>(sql, calendarEvent);
        }

        public async Task<bool> UpdateAsync(CalendarEvent calendarEvent)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            // Rettet til lowercase
            var sql = @"
                UPDATE calendarevents SET
                    title = @Title,
                    eventtype = @EventType,
                    starttime = @StartTime,
                    endtime = @EndTime,
                    notes = @Notes,
                    createdbyuserid = @CreatedByUserId
                WHERE id = @Id";

            var affectedRows = await connection.ExecuteAsync(sql, calendarEvent);
            return affectedRows > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            // Rettet til lowercase
            var sql = @"DELETE FROM calendarevents WHERE id = @Id";
            var affectedRows = await connection.ExecuteAsync(sql, new { Id = id });
            return affectedRows > 0;
        }
    }
}