using CiOHjemmeside.Components;
using CiOHjemmeside.Data.Services;
using Npgsql;
using Microsoft.AspNetCore.Components.Authorization;
using CiOHjemmeside.Data.Auth;

var builder = WebApplication.CreateBuilder(args);

// FJERNET: builder.Services.AddHttpContextAccessor();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// --- START: Auth-konfiguration (Fase 3 - Forenklet) ---

// FJERNET: builder.Services.AddAuthentication("CiO-Auth")...

// Tilføjer Authorization-services (til [Authorize] og roller)
builder.Services.AddAuthorizationCore();

// Gør auth-staten tilgængelig for alle komponenter via <CascadingAuthenticationState>
builder.Services.AddCascadingAuthenticationState();

// Registrer vores custom provider. Scoped er VIGTIGT.
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

// FJERNET: app.UseAuthentication();
// FJERNET: app.UseAuthorization();

// KORREKT KODE: Denne linje SKAL være her for at mappe SignalR-endpoints.
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();