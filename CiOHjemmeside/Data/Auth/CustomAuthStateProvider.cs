using CiOHjemmeside.Data.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Security.Claims;

namespace CiOHjemmeside.Data.Auth
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly IUserService _userService;
        private readonly ProtectedSessionStorage _sessionStorage;
        private ClaimsPrincipal _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
        private const string AuthStorageKey = "CiO-AuthState";

        public CustomAuthStateProvider(IUserService userService, ProtectedSessionStorage sessionStorage)
        {
            _userService = userService;
            _sessionStorage = sessionStorage;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                // Forsøg at hente auth state fra session storage
                var result = await _sessionStorage.GetAsync<string[]>(AuthStorageKey);

                if (result.Success && result.Value != null && result.Value.Length == 3)
                {
                    var claims = new[]
                    {
                        new Claim(ClaimTypes.Name, result.Value[0]),
                        new Claim(ClaimTypes.Role, result.Value[1]),
                        new Claim(ClaimTypes.NameIdentifier, result.Value[2])
                    };

                    var identity = new ClaimsIdentity(claims, "CiO-Auth", ClaimTypes.Name, ClaimTypes.Role);
                    _currentUser = new ClaimsPrincipal(identity);
                }
            }
            catch
            {
                // Hvis der er fejl ved at læse fra storage, fortsæt med tom bruger
            }

            return new AuthenticationState(_currentUser);
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            var user = await _userService.GetByUsernameAsync(username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return false;
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var identity = new ClaimsIdentity(claims, "CiO-Auth", ClaimTypes.Name, ClaimTypes.Role);
            _currentUser = new ClaimsPrincipal(identity);

            // Gem auth state i session storage
            await _sessionStorage.SetAsync(AuthStorageKey, new[] { user.Username, user.Role, user.Id.ToString() });

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));

            return true;
        }

        public async void Logout()
        {
            _currentUser = new ClaimsPrincipal(new ClaimsIdentity());

            // Fjern auth state fra session storage
            await _sessionStorage.DeleteAsync(AuthStorageKey);

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
        }
    }
}