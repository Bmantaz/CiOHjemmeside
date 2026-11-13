namespace CiOHjemmeside.Data.Models
{
    /// <summary>
    /// Opsummerer hvor meget hvert bandmedlem har solgt bag disken i en periode.
    /// Salg fra foer saelgeren blev registreret samles under "Ukendt".
    /// </summary>
    public class SellerSalesSummary
    {
        public string SellerName { get; set; } = "Ukendt";
        public int SaleCount { get; set; }
        public int TotalItemsSold { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
