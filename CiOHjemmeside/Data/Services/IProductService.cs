using CiOHjemmeside.Data.Models;

namespace CiOHjemmeside.Data.Services
{
    /// <summary>
    /// Definerer operationer for produkt- og variantstyring (POS-system).
    /// </summary>
    public interface IProductService
    {
        /// <summary>
        /// Henter alle produktgrupper (f.eks. "T-Shirt", "Vinyl").
        /// </summary>
        Task<IEnumerable<ProductGroup>> GetAllGroupsAsync();

        /// <summary>
        /// Henter alle varianter (f.eks. "S", "M", "L") for en specifik gruppe.
        /// </summary>
        Task<IEnumerable<ProductVariant>> GetVariantsByGroupIdAsync(int groupId);

        /// <summary>
        /// Henter alle grupper OG alle deres tilhørende varianter i ét kald.
        /// </summary>
        Task<IEnumerable<ProductGroup>> GetAllGroupsWithVariantsAsync();

        // CRUD for Varianter (til lageropdatering)
        Task<ProductVariant?> GetVariantByIdAsync(int variantId);
        Task<bool> UpdateVariantStockAsync(int variantId, int newStockQuantity);

        // Fuld CRUD (til admin-brug senere)
        Task<int> AddGroupAsync(ProductGroup group);
        Task<bool> UpdateGroupAsync(ProductGroup group);
        Task<bool> DeleteGroupAsync(int groupId);
        Task<int> AddVariantAsync(ProductVariant variant);
        Task<bool> UpdateVariantAsync(ProductVariant variant);
        Task<bool> DeleteVariantAsync(int variantId);
    }
}