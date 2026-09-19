using CiOHjemmeside.Data.Models;

namespace CiOHjemmeside.Data.Services
{
    public interface ISalesService
    {
        Task<List<ProductGroup>> GetAllProductGroupsWithVariantsAsync();
        Task RecordSaleAsync(int soldByUserId, IEnumerable<SaleItemInput> items, DateTime? soldAtUtc = null, int? concertId = null);
        Task<SalesStatisticsResult> GetStatisticsForDateAsync(DateTime date);
        Task<SalesStatisticsResult> GetStatisticsForRangeAsync(DateTime from, DateTime to);
        Task<List<DailySalesSummary>> GetDailySalesSummaryAsync(DateTime from, DateTime to);

        /// <summary>
        /// Opsummerer solgt merch pr. spillested/show i en given periode.
        /// Salg uden tilknyttet koncert samles under "Ukendt".
        /// </summary>
        Task<List<VenueSalesSummary>> GetSalesByVenueAsync(DateTime from, DateTime to);
    }
}