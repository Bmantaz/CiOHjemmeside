namespace CiOHjemmeside.Data.Services
{
    public interface IEpkAccessService
    {
        Task<bool> IsUnlockedAsync();

        /// <summary>
        /// Samlet adgangstjek til EPK: indlogget Admin/Member, eller adgangskoden
        /// er indtastet i denne browser-session.
        /// </summary>
        Task<bool> HasAccessAsync();
        Task<bool> ValidateAndUnlockAsync(string accessCode);
    }
}
