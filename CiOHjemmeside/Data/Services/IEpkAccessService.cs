namespace CiOHjemmeside.Data.Services
{
    public interface IEpkAccessService
    {
        Task<bool> IsUnlockedAsync();
        Task<bool> ValidateAndUnlockAsync(string accessCode);
    }
}
