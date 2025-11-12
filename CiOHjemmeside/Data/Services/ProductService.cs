using CiOHjemmeside.Data.Models;
using Dapper;

namespace CiOHjemmeside.Data.Services
{
    public class ProductService : IProductService
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public ProductService(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<ProductGroup>> GetAllGroupsAsync()
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            return await connection.QueryAsync<ProductGroup>("SELECT * FROM productgroups ORDER BY groupname");
        }

        public async Task<IEnumerable<ProductVariant>> GetVariantsByGroupIdAsync(int groupId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = "SELECT * FROM productvariants WHERE productgroupid = @GroupId ORDER BY variantname";
            return await connection.QueryAsync<ProductVariant>(sql, new { GroupId = groupId });
        }

        public async Task<IEnumerable<ProductGroup>> GetAllGroupsWithVariantsAsync()
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM productgroups g
                LEFT JOIN productvariants v ON g.id = v.productgroupid
                ORDER BY g.groupname, v.variantname";

            var groupDict = new Dictionary<int, ProductGroup>();

            var groups = await connection.QueryAsync<ProductGroup, ProductVariant, ProductGroup>(
                sql,
                (group, variant) =>
                {
                    if (!groupDict.TryGetValue(group.Id, out var currentGroup))
                    {
                        currentGroup = group;
                        groupDict.Add(currentGroup.Id, currentGroup);
                    }
                    if (variant != null)
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
                "SELECT * FROM productvariants WHERE id = @VariantId",
                new { VariantId = variantId });
        }

        public async Task<bool> UpdateVariantStockAsync(int variantId, int newStockQuantity)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = "UPDATE productvariants SET stockquantity = @Stock WHERE id = @Id";
            var affectedRows = await connection.ExecuteAsync(sql, new { Stock = newStockQuantity, Id = variantId });
            return affectedRows > 0;
        }

        // --- Fuld CRUD (implementeret til admin-brug) ---

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
            // Kaskadesletning (ON DELETE CASCADE) i SQL'en bør håndtere varianter
            var sql = "DELETE FROM productgroups WHERE id = @Id";
            var affectedRows = await connection.ExecuteAsync(sql, new { Id = groupId });
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

        public async Task<bool> DeleteVariantAsync(int variantId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = "DELETE FROM productvariants WHERE id = @Id";
            var affectedRows = await connection.ExecuteAsync(sql, new { Id = variantId });
            return affectedRows > 0;
        }
    }
}