using CiOHjemmeside.Data.Models;

namespace CiOHjemmeside.Data.Services
{
    public interface IEpkAssetService
    {
        Task<IEnumerable<EpkAsset>> GetByCategoryAsync(string category);
        Task<EpkAssetFile?> GetFileAsync(int id);
        Task<EpkAssetFile?> GetThumbnailAsync(int id);
        Task<int> AddAsync(string category, string title, string fileName, string contentType, byte[] content);
        Task UpdateTitleAsync(int id, string title);
        Task ReplaceFileAsync(int id, string fileName, string contentType, byte[] content);
        Task MoveAsync(int id, int direction);
        Task DeleteAsync(int id);
        Task<bool> HasAnyAsync();
    }
}
