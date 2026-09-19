using CiOHjemmeside.Data.Models;

namespace CiOHjemmeside.Data.Services
{
    /// <summary>
    /// Importerer de eksisterende statiske EPK-filer fra wwwroot/EPK ind i databasen,
    /// men KUN første gang (når epkassets-tabellen er tom). Derefter er databasen
    /// den eneste kilde til sandhed, og admin kan frit tilføje/redigere/slette.
    /// </summary>
    public static class EpkAssetSeeder
    {
        private record SeedItem(string Category, string Title, string FileName);

        private static readonly SeedItem[] Items =
        [
            new(EpkCategories.Riders, "Technical Rider", "technical-rider.pdf"),
            new(EpkCategories.Riders, "Hospitality Rider", "hospitality-rider.pdf"),

            new(EpkCategories.Press, "Press Text (English)", "press-text-en.pdf"),
            new(EpkCategories.Press, "Press Text (Dansk)", "press-text-da.pdf"),

            new(EpkCategories.ImagesBand, "Band Picture", "updated-band-picture-full.jpg"),
            new(EpkCategories.ImagesBand, "Band Picture With Logo", "updated-band-picture-full-with-logo.jpg"),

            new(EpkCategories.ImagesPosters, "Black Header Transparent", "black-header-transparent.png"),
            new(EpkCategories.ImagesPosters, "Black Logo Stacked Transparent", "black-logo-stacked-transparent.png"),
            new(EpkCategories.ImagesPosters, "Black Spear Logo Transparent", "black-spear-logo-transparent.png"),
            new(EpkCategories.ImagesPosters, "White Header Not Transparent", "white-header-not-transparent.jpg"),
            new(EpkCategories.ImagesPosters, "White Header Transparent", "white-header-transparent.png"),
            new(EpkCategories.ImagesPosters, "White Logo Stacked Transparent", "white-logo-stacked-transparent.png"),
            new(EpkCategories.ImagesPosters, "White Spear Logo Transparent", "white-spear-logo-transparent.png"),
            new(EpkCategories.ImagesPosters, "White Stacked Not Transparent", "white-stacked-not-transparent.jpg")
        ];

        public static async Task SeedAsync(IEpkAssetService assetService, string webRootPath, ILogger logger)
        {
            if (await assetService.HasAnyAsync())
            {
                return;
            }

            var sourceDirectory = Path.Combine(webRootPath, "EPK");
            if (!Directory.Exists(sourceDirectory))
            {
                logger.LogWarning("EPK seed skipped: {Directory} does not exist.", sourceDirectory);
                return;
            }

            foreach (var item in Items)
            {
                var path = Path.Combine(sourceDirectory, item.FileName);
                if (!File.Exists(path))
                {
                    logger.LogWarning("EPK seed skipped missing file {File}.", item.FileName);
                    continue;
                }

                var content = await File.ReadAllBytesAsync(path);
                await assetService.AddAsync(item.Category, item.Title, item.FileName, ResolveContentType(item.FileName), content);
            }

            logger.LogInformation("EPK assets seeded from {Directory}.", sourceDirectory);
        }

        private static string ResolveContentType(string fileName) =>
            Path.GetExtension(fileName).ToLowerInvariant() switch
            {
                ".pdf" => "application/pdf",
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };
    }
}
