using CiOHjemmeside.Data.Models;

namespace CiOHjemmeside.Data.Services
{
    public interface ISalesService
    {
        Task RecordSaleAsync(int soldByUserId, IEnumerable<SaleItemInput> items, DateTime? soldAtUtc = null);
        Task<SalesStatisticsResult> GetStatisticsForDateAsync(DateTime date);
    }
}
