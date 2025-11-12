using System.ComponentModel.DataAnnotations.Schema;

namespace CiOHjemmeside.Data.Models
{
    [Table("productgroups")]
    public class ProductGroup
    {
        public int Id { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }

        // Navigationsegenskab: En gruppe har mange varianter
        [NotMapped]
        public List<ProductVariant> Variants { get; set; } = new();
    }
}