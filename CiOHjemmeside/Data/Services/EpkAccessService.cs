using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Extensions.Configuration;

namespace CiOHjemmeside.Data.Services
{
    public class EpkAccessService : IEpkAccessService
    {
        private const string EpkAccessStorageKey = "CiO-EPK-Unlocked";

        private readonly ProtectedSessionStorage _sessionStorage;
        private readonly IConfiguration _configuration;

        public EpkAccessService(ProtectedSessionStorage sessionStorage, IConfiguration configuration)
        {
            _sessionStorage = sessionStorage;
            _configuration = configuration;
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
            var configuredAccessCode = _configuration["Security:EpkAccessCode"];
            var isValid = !string.IsNullOrWhiteSpace(configuredAccessCode)
                && string.Equals(accessCode?.Trim(), configuredAccessCode, StringComparison.Ordinal);
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
