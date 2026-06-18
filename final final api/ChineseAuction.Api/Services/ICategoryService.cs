using ChineseAuction.Api.Dtos;

namespace ChineseAuction.Api.Services
{
    public interface ICategoryService
    {
        Task<int> CreateAsync(CategoryDto dto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<CategoryDto>> GetAllAsync();
        Task<CategoryDto?> GetByIdAsync(int id);
        Task<bool> UpdateAsync(int id, CategoryDto dto);
    }
}