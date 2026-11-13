using CiOHjemmeside.Data.Models;
using Dapper;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace CiOHjemmeside.Data.Services
{
    public class ProductService : IProductService
    {
        // Produktbilleder vises som smaa kort paa salgssiden; 800px er rigeligt.
        private const int MaxImageSize = 800;

        private readonly IDbConnectionFactory _connectionFactory;

        public ProductService(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        // Bevidst eksplicitte kolonner: productgroups.imagedata maa aldrig hentes i lister.
        private const string GroupColumns = "g.id, g.groupname, g.imageurl";
        private const string VariantColumns = "v.id, v.productgroupid, v.variantname, v.stockquantity, v.price";

        public async Task<IEnumerable<ProductGroup>> GetAllGroupsAsync()
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            return await connection.QueryAsync<ProductGroup>(
                $"SELECT {GroupColumns} FROM productgroups g ORDER BY g.groupname");
        }

        public async Task<IEnumerable<ProductVariant>> GetVariantsByGroupIdAsync(int groupId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = $"SELECT {VariantColumns} FROM productvariants v WHERE v.productgroupid = @GroupId ORDER BY v.variantname";
            return await connection.QueryAsync<ProductVariant>(sql, new { GroupId = groupId });
        }

        public async Task<IEnumerable<ProductGroup>> GetAllGroupsWithVariantsAsync()
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = $@"
                SELECT {GroupColumns}, {VariantColumns}
                FROM productgroups g
                LEFT JOIN productvariants v ON g.id = v.productgroupid
                ORDER BY g.groupname, v.variantname";

            var groupDict = new Dictionary<int, ProductGroup>();

            await connection.QueryAsync<ProductGroup, ProductVariant, ProductGroup>(
                sql,
                (group, variant) =>
                {
                    if (!groupDict.TryGetValue(group.Id, out var currentGroup))
                    {
                        currentGroup = group;
                        groupDict.Add(currentGroup.Id, currentGroup);
                    }
                    if (variant != null && variant.Id > 0)
                    {
                        currentGroup.Variants.Add(variant);
                    }
                    return currentGroup;
                },
                splitOn: "id" // Splitter på variantens 'id'
            );
            return groupDict.Values;
        }

        public async Task<ProductVariant?> GetVariantByIdAsync(int variantId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            return await connection.QuerySingleOrDefaultAsync<ProductVariant>(
                $"SELECT {VariantColumns} FROM productvariants v WHERE v.id = @VariantId",
                new { VariantId = variantId });
        }

        public async Task<bool> UpdateVariantStockAsync(int variantId, int newStockQuantity)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = "UPDATE productvariants SET stockquantity = @Stock WHERE id = @Id";
            var affectedRows = await connection.ExecuteAsync(sql, new { Stock = newStockQuantity, Id = variantId });
            return affectedRows > 0;
        }

        // --- Fuld CRUD (admin) ---

        public async Task<int> AddGroupAsync(ProductGroup group)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = "INSERT INTO productgroups (groupname, imageurl) VALUES (@GroupName, @ImageUrl) RETURNING id";
            return await connection.QuerySingleAsync<int>(sql, group);
        }

        public async Task<bool> UpdateGroupAsync(ProductGroup group)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = "UPDATE productgroups SET groupname = @GroupName, imageurl = @ImageUrl WHERE id = @Id";
            var affectedRows = await connection.ExecuteAsync(sql, group);
            return affectedRows > 0;
        }

        public async Task<bool> DeleteGroupAsync(int groupId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var transaction = connection.BeginTransaction();

            // Slet varianterne eksplicit, saa vi ikke er afhaengige af en ON DELETE CASCADE,
            // som vi ikke ved om findes i den eksisterende tabel.
            await connection.ExecuteAsync(
                "DELETE FROM productvariants WHERE productgroupid = @Id", new { Id = groupId }, transaction);
            var affectedRows = await connection.ExecuteAsync(
                "DELETE FROM productgroups WHERE id = @Id", new { Id = groupId }, transaction);

            transaction.Commit();
            return affectedRows > 0;
        }

        public async Task<int> AddVariantAsync(ProductVariant variant)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                INSERT INTO productvariants (productgroupid, variantname, stockquantity, price)
                VALUES (@ProductGroupId, @VariantName, @StockQuantity, @Price) RETURNING id";
            return await connection.QuerySingleAsync<int>(sql, variant);
        }

        public async Task<bool> UpdateVariantAsync(ProductVariant variant)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                UPDATE productvariants SET
                    productgroupid = @ProductGroupId,
                    variantname = @VariantName,
                    stockquantity = @StockQuantity,
                    price = @Price
                WHERE id = @Id";
            var affectedRows = await connection.ExecuteAsync(sql, variant);
            return affectedRows > 0;
        }

        public async Task<bool> SaveVariantAsync(int variantId, string variantName, decimal? price, int stockDelta)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                UPDATE productvariants SET
                    variantname = @VariantName,
                    price = @Price,
                    stockquantity = GREATEST(stockquantity + @StockDelta, 0)
                WHERE id = @Id";
            var affectedRows = await connection.ExecuteAsync(sql, new
            {
                Id = variantId,
                VariantName = variantName,
                Price = price,
                StockDelta = stockDelta
            });
            return affectedRows > 0;
        }

        public async Task<bool> DeleteVariantAsync(int variantId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = "DELETE FROM productvariants WHERE id = @Id";
            var affectedRows = await connection.ExecuteAsync(sql, new { Id = variantId });
            return affectedRows > 0;
        }

        public async Task<bool> GroupNameExistsAsync(string groupName, int? excludeGroupId = null)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            return await connection.ExecuteScalarAsync<bool>(
                @"SELECT EXISTS (
                      SELECT 1 FROM productgroups
                      WHERE LOWER(TRIM(groupname)) = LOWER(TRIM(@Name))
                        AND (@ExcludeId IS NULL OR id <> @ExcludeId))",
                new { Name = groupName, ExcludeId = excludeGroupId });
        }

        public async Task SetGroupImageAsync(int groupId, byte[] originalImage)
        {
            // Kaster en exception hvis filen ikke er et gyldigt billede; siden viser fejlen.
            var compressed = CompressImage(originalImage);

            using var connection = await _connectionFactory.CreateConnectionAsync();

            // ?v= tvinger browseren til at hente det nye billede efter en udskiftning.
            var imageUrl = $"/product-image/{groupId}?v={DateTime.UtcNow.Ticks:x}";

            await connection.ExecuteAsync(
                @"UPDATE productgroups
                  SET imagedata = @Data, imagecontenttype = @ContentType, imageurl = @ImageUrl
                  WHERE id = @Id",
                new { Id = groupId, Data = compressed, ContentType = "image/webp", ImageUrl = imageUrl });
        }

        public async Task<ProductImage?> GetGroupImageAsync(int groupId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            return await connection.QuerySingleOrDefaultAsync<ProductImage>(
                @"SELECT imagedata AS content, imagecontenttype AS contenttype
                  FROM productgroups
                  WHERE id = @Id AND imagedata IS NOT NULL",
                new { Id = groupId });
        }

        /// <summary>
        /// Skalerer ned til maks 800px og gemmer som WebP. WebP bevarer transparens
        /// og goer typisk et 20-30 MB foto til et par hundrede KB.
        /// </summary>
        private static byte[] CompressImage(byte[] content)
        {
            using var image = Image.Load(content);

            image.Mutate(x => x.AutoOrient().Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(MaxImageSize, MaxImageSize)
            }));

            using var output = new MemoryStream();
            image.Save(output, new WebpEncoder { Quality = 85 });
            return output.ToArray();
        }
    }
}
