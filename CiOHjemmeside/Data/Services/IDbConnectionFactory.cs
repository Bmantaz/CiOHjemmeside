using System.Data;

namespace CiOHjemmeside.Data.Services
{
    /// <summary>
    /// En factory til at oprette og åbne en databaseforbindelse.
    /// </summary>
    public interface IDbConnectionFactory
    {
        /// <summary>
        /// Opretter og åbner asynkront en ny IDbConnection.
        /// </summary>
        Task<IDbConnection> CreateConnectionAsync();
    }
}