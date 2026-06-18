using ChineseAuction.Api.Models;

namespace ChineseAuction.Api.Repositories
{
    public interface IGiftRepository
    {
        Task<int> CreateAsync(Gift gift);
        Task<bool> DeleteAsync(int id);
        Task<bool> DonorExistsAsync(int donorId);
        Task<IEnumerable<Gift>> GetAllAsync();
        Task<Gift?> GetByIdAsync(int id);
        Task<Gift?> GetByIdTrackedAsync(int id);
        Task SaveChangesAsync();
        Task<IEnumerable<Gift>> SearchGiftsInternalAsync(string? name, string? donor, int? minPurchasers);
        Task<IEnumerable<Gift>> GetAllSortedByPriceAsync(bool ascending);
        Task<IEnumerable<Gift>> GetAllSortedByCategoryAsync();
    }
}