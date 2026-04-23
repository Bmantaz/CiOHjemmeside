using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace CiOHjemmeside.Data.Services
{
    public class EpkAccessService : IEpkAccessService
    {
        private const string EpkAccessStorageKey = "CiO-EPK-Unlocked";
        private const string EpkAccessCode = "1379";

        private readonly ProtectedSessionStorage _sessionStorage;

        public EpkAccessService(ProtectedSessionStorage sessionStorage)
        {
            _sessionStorage = sessionStorage;
        }

        public async Task<bool> IsUnlockedAsync()
        {
            try
            {
                var result = await _sessionStorage.GetAsync<bool>(EpkAccessStorageKey);
                return result.Success && result.Value;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ValidateAndUnlockAsync(string accessCode)
        {
            var isValid = string.Equals(accessCode?.Trim(), EpkAccessCode, StringComparison.Ordinal);
            if (!isValid)
            {
                return false;
            }

            try
            {
                await _sessionStorage.SetAsync(EpkAccessStorageKey, true);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
