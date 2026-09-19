using CiOHjemmeside.Data.Models;
using Dapper;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

namespace CiOHjemmeside.Data.Services
{
    public class EpkAssetService : IEpkAssetService
    {
        private const int ThumbnailSize = 220;

        private readonly IDbConnectionFactory _connectionFactory;

        public EpkAssetService(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<EpkAsset>> GetByCategoryAsync(string category)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();

            // Bevidst UDEN content/thumbnail, så en liste aldrig henter binære data.
            var sql = @"
                SELECT
                    id,
                    category,
                    title,
                    filename,
                    contenttype,
                    filesize,
                    (thumbnail IS NOT NULL) AS hasthumbnail,
                    sortorder,
                    uploadedat
                FROM epkassets
                WHERE category = @Category
                ORDER BY sortorder ASC, id ASC";

            return await connection.QueryAsync<EpkAsset>(sql, new { Category = category });
        }

        public async Task<EpkAssetFile?> GetFileAsync(int id)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();

            var sql = @"
                SELECT content, contenttype, filename
                FROM epkassets
                WHERE id = @Id";

            return await connection.QuerySingleOrDefaultAsync<EpkAssetFile>(sql, new { Id = id });
        }

        public async Task<EpkAssetFile?> GetThumbnailAsync(int id)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();

            var sql = @"
                SELECT thumbnail AS content, 'image/png' AS contenttype, filename
                FROM epkassets
                WHERE id = @Id AND thumbnail IS NOT NULL";

            return await connection.QuerySingleOrDefaultAsync<EpkAssetFile>(sql, new { Id = id });
        }

        public async Task<int> AddAsync(string category, string title, string fileName, string contentType, byte[] content)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();

            var nextSortOrder = await connection.ExecuteScalarAsync<int>(
                "SELECT COALESCE(MAX(sortorder), 0) + 1 FROM epkassets WHERE category = @Category",
                new { Category = category });

            var sql = @"
                INSERT INTO epkassets (category, title, filename, contenttype, filesize, content, thumbnail, sortorder, uploadedat)
                VALUES (@Category, @Title, @FileName, @ContentType, @FileSize, @Content, @Thumbnail, @SortOrder, @UploadedAt)
                RETURNING id";

            return await connection.QuerySingleAsync<int>(sql, new
            {
                Category = category,
                Title = title,
                FileName = fileName,
                ContentType = contentType,
                FileSize = (long)content.Length,
                Content = content,
                Thumbnail = CreateThumbnail(content, contentType),
                SortOrder = nextSortOrder,
                UploadedAt = DateTime.UtcNow
            });
        }

        public async Task UpdateTitleAsync(int id, string title)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();

            await connection.ExecuteAsync(
                "UPDATE epkassets SET title = @Title WHERE id = @Id",
                new { Id = id, Title = title });
        }

        public async Task ReplaceFileAsync(int id, string fileName, string contentType, byte[] content)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();

            var sql = @"
                UPDATE epkassets
                SET filename = @FileName,
                    contenttype = @ContentType,
                    filesize = @FileSize,
                    content = @Content,
                    thumbnail = @Thumbnail,
                    uploadedat = @UploadedAt
                WHERE id = @Id";

            await connection.ExecuteAsync(sql, new
            {
                Id = id,
                FileName = fileName,
                ContentType = contentType,
                FileSize = (long)content.Length,
                Content = content,
                Thumbnail = CreateThumbnail(content, contentType),
                UploadedAt = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Flytter et asset op (-1) eller ned (+1) ved at bytte sortorder med naboen.
        /// </summary>
        public async Task MoveAsync(int id, int direction)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();

            var current = await connection.QuerySingleOrDefaultAsync<SortRow>(
                "SELECT id, category, sortorder FROM epkassets WHERE id = @Id",
                new { Id = id });

            if (current is null)
            {
                return;
            }

            var neighbourSql = direction < 0
                ? @"SELECT id, category, sortorder FROM epkassets
                    WHERE category = @Category AND sortorder < @SortOrder
                    ORDER BY sortorder DESC LIMIT 1"
                : @"SELECT id, category, sortorder FROM epkassets
                    WHERE category = @Category AND sortorder > @SortOrder
                    ORDER BY sortorder ASC LIMIT 1";

            var neighbour = await connection.QuerySingleOrDefaultAsync<SortRow>(neighbourSql, new
            {
                current.Category,
                current.SortOrder
            });

            if (neighbour is null)
            {
                return;
            }

            await connection.ExecuteAsync(
                "UPDATE epkassets SET sortorder = @SortOrder WHERE id = @Id",
                new { Id = current.Id, SortOrder = neighbour.SortOrder });
            await connection.ExecuteAsync(
                "UPDATE epkassets SET sortorder = @SortOrder WHERE id = @Id",
                new { Id = neighbour.Id, SortOrder = current.SortOrder });
        }

        private sealed class SortRow
        {
            public int Id { get; set; }
            public string Category { get; set; } = string.Empty;
            public int SortOrder { get; set; }
        }

        public async Task DeleteAsync(int id)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();

            await connection.ExecuteAsync("DELETE FROM epkassets WHERE id = @Id", new { Id = id });
        }

        public async Task<bool> HasAnyAsync()
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();

            return await connection.ExecuteScalarAsync<bool>("SELECT EXISTS (SELECT 1 FROM epkassets)");
        }

        /// <summary>
        /// Genererer et PNG-thumbnail for billeder. PNG bevarer transparens, hvilket er
        /// nødvendigt for logo-filerne. Returnerer null for ikke-billeder (fx PDF)
        /// eller hvis filen ikke kan afkodes som et billede.
        /// </summary>
        private static byte[]? CreateThumbnail(byte[] content, string contentType)
        {
            if (!contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            try
            {
                using var image = Image.Load(content);

                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Max,
                    Size = new Size(ThumbnailSize, ThumbnailSize)
                }));

                using var output = new MemoryStream();
                image.Save(output, new PngEncoder());
                return output.ToArray();
            }
            catch (Exception)
            {
                // Ugyldigt/ikke-understøttet billedformat: gem uden thumbnail.
                return null;
            }
        }
    }
}
