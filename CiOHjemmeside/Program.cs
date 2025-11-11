using CiOHjemmeside.Components;
using CiOHjemmeside.Data.Services; // Tilføjet for services
using Npgsql; // Tilføjet for NpgsqlDataSource

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents();

// --- START: Konfiguration af Data-lag (Fase 1) ---

// 1. Hent Connection String fra appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                     ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// 2. Registrer NpgsqlDataSource som en singleton (anbefalet praksis for .NET 8)
// Dette håndterer connection pooling effektivt.
builder.Services.AddNpgsqlDataSource(connectionString);

// 3. Registrer vores DbConnectionFactory (som nu afhænger af NpgsqlDataSource)
// Vi bruger Singleton, da dens eneste afhængighed (DataSource) også er Singleton.
builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();

// 4. Registrer vores services/repositories. 
// Scoped er passende for Blazor Server, da det sikrer en ny instans pr. bruger-session/request.
builder.Services.AddScoped<IConcertService, ConcertService>();
builder.Services.AddScoped<IMerchandiseService, MerchandiseService>();
// NY REGISTRERING TILFØJET:
builder.Services.AddScoped<ICalendarEventService, CalendarEventService>();

// --- SLUT: Konfiguration af Data-lag (Fase 1) ---


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>();

app.Run();