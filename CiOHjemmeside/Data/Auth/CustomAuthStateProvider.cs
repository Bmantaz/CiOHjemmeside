using CiOHjemmeside.Data.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace CiOHjemmeside.Data.Auth
{
    // Denne provider håndterer nu BÅDE Blazor state og HTTP Cookie state
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CustomAuthStateProvider(IUserService userService, IHttpContextAccessor httpContextAccessor)
        {
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            // Henter brugeren fra HTTP-kontekstens cookie
            var user = _httpContextAccessor.HttpContext?.User;

            if (user == null || !user.Identity.IsAuthenticated)
            {
                // Bruger er anonym
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            // Bruger er logget ind
            return new AuthenticationState(user);
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            var user = await _userService.GetByUsernameAsync(username);
            if (user == null)
            {
                return false; // Bruger findes ikke
            }

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return false; // Forkert adgangskode
            }

            // Opret claims (rettigheder)
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role), // "Admin", "Member", "Sales"
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var identity = new ClaimsIdentity(claims, "CiO-Auth"); // Matcher "CiO-Auth" fra Program.cs
            var principal = new ClaimsPrincipal(identity);

            // DEN VIGTIGE DEL: Logger brugeren ind i cookie-systemet
            if (_httpContextAccessor.HttpContext != null)
            {
                await _httpContextAccessor.HttpContext.SignInAsync("CiO-Auth", principal);
            }

            // Notificer Blazor om, at staten har ændret sig
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));

            return true;
        }

        public async Task Logout()
        {
            // Log ud af cookie-systemet
            if (_httpContextAccessor.HttpContext != null)
            {
                await _httpContextAccessor.HttpContext.SignOutAsync("CiO-Auth");
            }

            // Notificer Blazor om, at staten har ændret sig (til anonym)
            var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymous)));
        }
    }
}