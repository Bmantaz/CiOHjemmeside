using System.ComponentModel.DataAnnotations.Schema;

namespace CiOHjemmeside.Data.Models
{
    // Rettet fra [Table("Merchandise")] til [Table("merchandise")]
    [Table("merchandise")]
    public class MerchandiseItem
    {
        public int Id { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int StockQuantity { get; set; }
        public decimal? Price { get; set; }
    }
}