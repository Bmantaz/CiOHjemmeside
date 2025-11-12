using CiOHjemmeside.Data.Models;

namespace CiOHjemmeside.Data.Services
{
    /// <summary>
    /// Definerer operationer for Users-tabellen (brugerstyring og login).
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Henter alle brugere i systemet.
        /// </summary>
        Task<IEnumerable<User>> GetAllAsync();

        /// <summary>
        /// Henter en specifik bruger via ID.
        /// </summary>
        Task<User?> GetByIdAsync(int id);

        /// <summary>
        /// Henter en specifik bruger via brugernavn (case-sensitiv).
        /// Bruges til login-validering.
        /// </summary>
        Task<User?> GetByUsernameAsync(string username);

        /// <summary>
        /// Tilføjer en ny bruger til databasen.
        /// </summary>
        /// <returns>Returnerer ID'et på den nyoprettede bruger.</returns>
        Task<int> AddAsync(User user);

        /// <summary>
        /// Opdaterer en eksisterende bruger.
        /// </summary>
        /// <returns>Returnerer true, hvis opdateringen lykkedes.</returns>
        Task<bool> UpdateAsync(User user);

        /// <summary>
        /// Sletter en bruger fra databasen via ID.
        /// </summary>
        /// <returns>Returnerer true, hvis sletningen lykkedes.</returns>
        Task<bool> DeleteAsync(int id);
    }
}