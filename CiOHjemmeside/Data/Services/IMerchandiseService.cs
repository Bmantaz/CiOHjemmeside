using CiOHjemmeside.Data.Models;

namespace CiOHjemmeside.Data.Services
{
    /// <summary>
    /// Definerer operationer for Merchandise-tabellen (lagerstyring).
    /// </summary>
    public interface IMerchandiseService
    {
        /// <summary>
        /// Henter alle merchandise-emner.
        /// </summary>
        Task<IEnumerable<MerchandiseItem>> GetAllAsync();

        /// <summary>
        /// Henter et specifikt merchandise-emne via ID.
        /// </summary>
        Task<MerchandiseItem?> GetByIdAsync(int id);

        /// <summary>
        /// Tilføjer et nyt merchandise-emne til databasen.
        /// </summary>
        /// <returns>Returnerer ID'et på det nyoprettede emne.</returns>
        Task<int> AddAsync(MerchandiseItem item);

        /// <summary>
        /// Opdaterer et eksisterende merchandise-emne.
        /// </summary>
        /// <returns>Returnerer true, hvis opdateringen lykkedes.</returns>
        Task<bool> UpdateAsync(MerchandiseItem item);

        /// <summary>
        /// Sletter et merchandise-emne fra databasen via ID.
        /// </summary>
        /// <returns>Returnerer true, hvis sletningen lykkedes.</returns>
        Task<bool> DeleteAsync(int id);
    }
}