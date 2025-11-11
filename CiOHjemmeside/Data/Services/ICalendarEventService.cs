using CiOHjemmeside.Data.Models;

namespace CiOHjemmeside.Data.Services
{
    /// <summary>
    /// Definerer operationer for CalendarEvents-tabellen.
    /// </summary>
    public interface ICalendarEventService
    {
        /// <summary>
        /// Henter alle kalender-events inden for en given tidsperiode.
        /// </summary>
        Task<IEnumerable<CalendarEvent>> GetEventsForPeriodAsync(DateTime startTime, DateTime endTime);

        /// <summary>
        /// Henter en specifik kalender-event via ID.
        /// </summary>
        Task<CalendarEvent?> GetByIdAsync(int id);

        /// <summary>
        /// Tilføjer en ny kalender-event til databasen.
        /// </summary>
        /// <returns>Returnerer ID'et på den nyoprettede event.</returns>
        Task<int> AddAsync(CalendarEvent calendarEvent);

        /// <summary>
        /// Opdaterer en eksisterende kalender-event.
        /// </summary>
        /// <returns>Returnerer true, hvis opdateringen lykkedes.</returns>
        Task<bool> UpdateAsync(CalendarEvent calendarEvent);

        /// <summary>
        /// Sletter en kalender-event fra databasen via ID.
        /// </summary>
        /// <returns>Returnerer true, hvis sletningen lykkedes.</returns>
        Task<bool> DeleteAsync(int id);
    }
}