using CiOHjemmeside.Components;
using CiOHjemmeside.Data.Services;
using Npgsql;
using Microsoft.AspNetCore.Components.Authorization;
using CiOHjemmeside.Data.Auth;

var builder = WebApplication.CreateBuilder(args);

// FJERNET: builder.Services.AddHttpContextAccessor();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddHubOptions(options =>
    {
        // Hævet fra standard 32 KB, så admin kan uploade EPK-filer (PDF/billeder) via SignalR.
        options.MaximumReceiveMessageSize = 12 * 1024 * 1024;
    });

// --- START: Auth-konfiguration (Fase 3 - Forenklet) ---

// FJERNET: builder.Services.AddAuthentication("CiO-Auth")...

// Tilføjer Authorization-services (til [Authorize] og roller)
builder.Services.AddAuthorizationCore();

// Gør auth-staten tilgængelig for alle komponenter via <CascadingAuthenticationState>
builder.Services.AddCascadingAuthenticationState();

// Registrer vores custom provider. Scoped er VIGTIGT.
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped<IAuthService>(sp => (CustomAuthStateProvider)sp.GetRequiredService<AuthenticationStateProvider>());

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
builder.Services.AddScoped<ISalesService, SalesService>();
builder.Services.AddScoped<IEpkAccessService, EpkAccessService>();
builder.Services.AddScoped<IEpkAssetService, EpkAssetService>();
builder.Services.AddSingleton<IEpkFileUrlService, EpkFileUrlService>();
builder.Services.AddMemoryCache(); // EPK-previews caches i hukommelsen
// --- SLUT: Konfiguration af Data-lag (Fase 1) ---


var app = builder.Build();

// Kør al schema-DDL én gang ved opstart, i stedet for på hvert enkelt kald
using (var scope = app.Services.CreateScope())
{
    var connectionFactory = scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>();
    await DatabaseInitializer.InitializeAsync(connectionFactory);

    var assetService = scope.ServiceProvider.GetRequiredService<IEpkAssetService>();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("EpkAssetSeeder");
    await EpkAssetSeeder.SeedAsync(assetService, Path.Combine(app.Environment.ContentRootPath, "EpkSeed"), logger);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
// EPK-filerne laa tidligere offentligt i wwwroot/EPK. Bloker stien, hvis en gammel
// deploy har efterladt filerne paa serveren. Blazor-ruterne (/epk/riders osv.) har
// ingen filendelse og rammes derfor ikke.
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value ?? string.Empty;
    if (path.StartsWith("/EPK/", StringComparison.OrdinalIgnoreCase) && Path.HasExtension(path))
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return;
    }
    await next();
});

app.UseStaticFiles();
app.UseAntiforgery();

// FJERNET: app.UseAuthentication();
// FJERNET: app.UseAuthorization();

// KORREKT KODE: Denne linje SKAL være her for at mappe SignalR-endpoints.
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Serverer EPK-filer og thumbnails direkte fra databasen. Kraever et signeret link (?t=),
// som kun udstedes til brugere med EPK-adgang. Uden gyldigt link: 404.
app.MapGet("/epk-file/{id:int}", async (int id, string? t, IEpkAssetService assetService, IEpkFileUrlService epkUrls) =>
{
    if (!epkUrls.IsValid(EpkFileUrlService.FileKind, id, t))
    {
        return Results.NotFound();
    }

    var file = await assetService.GetFileAsync(id);
    return file is null
        ? Results.NotFound()
        : Results.File(file.Content, file.ContentType, file.FileName);
});

app.MapGet("/epk-thumb/{id:int}", async (int id, string? t, IEpkAssetService assetService, IEpkFileUrlService epkUrls) =>
{
    if (!epkUrls.IsValid(EpkFileUrlService.ThumbKind, id, t))
    {
        return Results.NotFound();
    }

    var thumb = await assetService.GetThumbnailAsync(id);
    return thumb is null
        ? Results.NotFound()
        : Results.File(thumb.Content, thumb.ContentType);
});

// Serverer produktbilleder (komprimeret WebP) fra databasen til salgssiden.
app.MapGet("/product-image/{id:int}", async (int id, IProductService productService) =>
{
    var image = await productService.GetGroupImageAsync(id);
    return image is null
        ? Results.NotFound()
        : Results.File(image.Content, image.ContentType);
});

app.Run();