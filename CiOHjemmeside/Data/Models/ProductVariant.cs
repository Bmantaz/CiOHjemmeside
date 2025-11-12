using System.ComponentModel.DataAnnotations.Schema;

namespace CiOHjemmeside.Data.Models
{
    [Table("productvariants")]
    public class ProductVariant
    {
        public int Id { get; set; }

        [Column("productgroupid")] // Sikrer korrekt mapping
        public int ProductGroupId { get; set; }

        public string VariantName { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
        public decimal? Price { get; set; }
    }
}