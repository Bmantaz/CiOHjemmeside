using CiOHjemmeside.Components;
using CiOHjemmeside.Data.Services;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// RETTET: Tilføjet .AddInteractiveServerComponents()
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// --- START: Konfiguration af Data-lag (Fase 1) ---

// 1. Hent Connection String fra appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                     ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// 2. Registrer NpgsqlDataSource som en singleton (anbefalet praksis for .NET 8)
builder.Services.AddNpgsqlDataSource(connectionString);

// 3. Registrer vores DbConnectionFactory
builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();

// 4. Registrer vores services/repositories. 
builder.Services.AddScoped<IConcertService, ConcertService>();
builder.Services.AddScoped<IMerchandiseService, MerchandiseService>();
builder.Services.AddScoped<ICalendarEventService, CalendarEventService>();
builder.Services.AddScoped<IUserService, UserService>();

// --- SLUT: Konfiguration af Data-lag (Fase 1) ---


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

// RETTET: Tilføjet .AddInteractiveServerRenderMode()
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();