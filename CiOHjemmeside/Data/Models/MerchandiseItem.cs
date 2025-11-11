using System.ComponentModel.DataAnnotations.Schema;

namespace CiOHjemmeside.Data.Models
{
    // Da tabellen hedder 'Merchandise' og klassen hedder 'MerchandiseItem',
    // tilføjer vi [Table] attributten for at sikre korrekt mapping.
    [Table("Merchandise")]
    public class MerchandiseItem
    {
        public int Id { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int StockQuantity { get; set; }
        public decimal? Price { get; set; }
    }
}