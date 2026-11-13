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

        // Fuld CRUD (admin)
        Task<int> AddGroupAsync(ProductGroup group);
        Task<bool> UpdateGroupAsync(ProductGroup group);

        /// <summary>
        /// Sletter en produktgruppe og alle dens varianter. Salgshistorikken
        /// bevares, da salgslinjer gemmer produktnavnet som tekst.
        /// </summary>
        Task<bool> DeleteGroupAsync(int groupId);

        Task<int> AddVariantAsync(ProductVariant variant);
        Task<bool> UpdateVariantAsync(ProductVariant variant);

        /// <summary>
        /// Gemmer navn og pris, og justerer lageret RELATIVT med stockDelta.
        /// Saa overskrives et salg, der sker mens admin redigerer, ikke.
        /// </summary>
        Task<bool> SaveVariantAsync(int variantId, string variantName, decimal? price, int stockDelta);

        Task<bool> DeleteVariantAsync(int variantId);

        /// <summary>
        /// Tjekker om et gruppenavn allerede er i brug (uden hensyn til store/smaa bogstaver).
        /// Salg matcher produkter paa navn, saa navne skal vaere unikke.
        /// </summary>
        Task<bool> GroupNameExistsAsync(string groupName, int? excludeGroupId = null);

        /// <summary>
        /// Skalerer og komprimerer et uploadet billede, gemmer det i databasen
        /// og peger gruppens imageurl paa det.
        /// </summary>
        Task SetGroupImageAsync(int groupId, byte[] originalImage);

        Task<ProductImage?> GetGroupImageAsync(int groupId);
    }
}
