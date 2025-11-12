using CiOHjemmeside.Components;
using CiOHjemmeside.Data.Services;
using Npgsql;
using Microsoft.AspNetCore.Components.Authorization;
using CiOHjemmeside.Data.Auth;

var builder = WebApplication.CreateBuilder(args);

// TILFØJET: Giver vores AuthProvider adgang til HttpContext
builder.Services.AddHttpContextAccessor();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// --- START: Auth-konfiguration (Fase 3) ---
builder.Services.AddAuthentication("CiO-Auth")
    .AddCookie("CiO-Auth", options =>
    {
        options.LoginPath = "/login"; // Fortæller systemet, hvor login-siden er
    });

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
// --- SLUT: Auth-konfiguration ---


// --- START: Konfiguration af Data-lag (Fase 1) ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                     ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddNpgsqlDataSource(connectionString);
builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();

builder.Services.AddScoped<IConcertService, ConcertService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICalendarEventService, CalendarEventService>();
builder.Services.AddScoped<IUserService, UserService>();
// --- SLUT: Konfiguration af Data-lag (Fase 1) ---


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();