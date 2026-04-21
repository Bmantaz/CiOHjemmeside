namespace CiOHjemmeside.Data.Models
{
    public class SaleItemInput
    {
        public string ProductGroupName { get; set; } = string.Empty;
        public string VariantName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
