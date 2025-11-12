using CiOHjemmeside.Data.Services;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace CiOHjemmeside.Data.Auth
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly IUserService _userService;
        private ClaimsPrincipal _currentUser = new ClaimsPrincipal(new ClaimsIdentity());

        // IUserService injiceres (Provideren er Scoped)
        public CustomAuthStateProvider(IUserService userService)
        {
            _userService = userService;
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            // Returnerer den aktuelt gemte bruger
            return Task.FromResult(new AuthenticationState(_currentUser));
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            var user = await _userService.GetByUsernameAsync(username);
            if (user == null)
            {
                return false; // Bruger findes ikke
            }

            // Valider adgangskode med BCrypt (den pakke vi tilføjede til seeding)
            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return false; // Forkert adgangskode
            }

            // Opret claims (rettigheder) baseret på data fra databasen
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role), // Vores Admin/Member/Sales rolle
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) // Brugerens ID
            };

            // Opret identitet og principal
            var identity = new ClaimsIdentity(claims, "CiO-Auth"); // Navngiver vores auth-metode
            _currentUser = new ClaimsPrincipal(identity);

            // Notificer systemet (alle [Authorize] tags) om, at login-staten har ændret sig
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));

            return true;
        }

        public void Logout()
        {
            // Nulstil til en anonym bruger
            _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
        }
    }
}