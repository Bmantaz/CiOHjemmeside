namespace CiOHjemmeside.Data.Models
{
    public class SalesStatisticsResult
    {
        public DateTime Date { get; set; }
        public List<SalesStatisticRow> Rows { get; set; } = new();
        public int TotalItemsSold { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
