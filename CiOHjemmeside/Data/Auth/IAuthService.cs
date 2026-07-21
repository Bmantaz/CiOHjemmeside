namespace CiOHjemmeside.Data.Auth
{
    /// <summary>
    /// Abstraction over login/logout so consumers (Razor pages/layouts) don't need to
    /// cast AuthenticationStateProvider to the concrete CustomAuthStateProvider type.
    /// </summary>
    public interface IAuthService
    {
        Task<bool> LoginAsync(string username, string password);
        Task LogoutAsync();
    }
}
