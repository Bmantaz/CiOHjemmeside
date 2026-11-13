using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Extensions.Configuration;

namespace CiOHjemmeside.Data.Services
{
    public class EpkAccessService : IEpkAccessService
    {
        private const string EpkAccessStorageKey = "CiO-EPK-Unlocked";

        private readonly ProtectedSessionStorage _sessionStorage;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EpkAccessService> _logger;
        private readonly AuthenticationStateProvider _authStateProvider;

        public EpkAccessService(
            ProtectedSessionStorage sessionStorage,
            IConfiguration configuration,
            ILogger<EpkAccessService> logger,
            AuthenticationStateProvider authStateProvider)
        {
            _sessionStorage = sessionStorage;
            _configuration = configuration;
            _logger = logger;
            _authStateProvider = authStateProvider;
        }

        public async Task<bool> HasAccessAsync()
        {
            var user = (await _authStateProvider.GetAuthenticationStateAsync()).User;
            if (user.Identity?.IsAuthenticated == true && (user.IsInRole("Admin") || user.IsInRole("Member")))
            {
                return true;
            }

            return await IsUnlockedAsync();
        }

        public async Task<bool> IsUnlockedAsync()
        {
            try
            {
                var result = await _sessionStorage.GetAsync<bool>(EpkAccessStorageKey);
                return result.Success && result.Value;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Kunne ikke læse EPK-adgangsstatus fra session storage.");
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
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Kunne ikke gemme EPK-adgangsstatus i session storage.");
                return false;
            }
        }
    }
}
