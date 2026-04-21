namespace CiOHjemmeside.Data.Models
{
    public class SalesStatisticRow
    {
        public string ProductGroupName { get; set; } = string.Empty;
        public string VariantName { get; set; } = string.Empty;
        public int TotalSold { get; set; }
        public decimal Revenue { get; set; }
    }
}
