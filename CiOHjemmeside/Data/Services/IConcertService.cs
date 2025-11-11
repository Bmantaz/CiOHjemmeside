using CiOHjemmeside.Data.Models;

namespace CiOHjemmeside.Data.Services
{
    /// <summary>
    /// Definerer operationer relateret til Concerts-tabellen.
    /// </summary>
    public interface IConcertService
    {
        /// <summary>
        /// Henter alle kommende koncerter (fra dags dato og frem).
        /// </summary>
        Task<IEnumerable<Concert>> GetUpcomingConcertsAsync();
    }
}