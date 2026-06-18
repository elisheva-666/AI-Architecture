using AutoMapper;
using ChineseAuction.Api.Dtos;
using ChineseAuction.Api.Models;
using ChineseAuction.Api.Repositories;
using Microsoft.Extensions.Logging;

namespace ChineseAuction.Api.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repo;
        private readonly IMapper _mapper;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(ICategoryRepository repo, IMapper mapper, ILogger<CategoryService> logger)
        {
            _repo = repo;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// מושך את כל הקטגוריות
        /// </summary>
        public async Task<IEnumerable<CategoryDto>> GetAllAsync()
        {
            var categories = await _repo.GetAllAsync();
            return _mapper.Map<IEnumerable<CategoryDto>>(categories);
        }

        /// <summary>
        /// מושך קטגוריה לפי id
        /// </summary>
        public async Task<CategoryDto?> GetByIdAsync(int id)
        {
            var category = await _repo.GetByIdAsync(id);
            return _mapper.Map<CategoryDto>(category);
        }

        /// <summary>
        /// יוצר קטגוריה חדשה עם בדיקות עסקיות
        /// </summary>
        public async Task<int> CreateAsync(CategoryDto dto)
        {
            var name = dto.Name?.Trim();
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("שם קטגוריה הוא חובה");

            if (await _repo.ExistsByNameAsync(name))
                throw new InvalidOperationException("קטגוריה עם שם זה כבר קיימת");

            var category = _mapper.Map<Category>(dto);
            var result = await _repo.CreateAsync(category);
            return result.Id;
        }

        /// <summary>
        /// מעדכן קטגוריה לפי id
        /// </summary>
        public async Task<bool> UpdateAsync(int id, CategoryDto dto)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return false;

            var newName = dto.Name?.Trim();
            if (await _repo.ExistsByNameAsync(newName!, id))
                throw new InvalidOperationException("קייימת קטגוריה אחרת עם שם זה");

            _mapper.Map(dto, existing); // מעדכן את האובייקט הקיים מה-DTO
            await _repo.UpdateAsync(existing);
            return true;
        }

        /// <summary>
        /// מוחק קטגוריה לפי id
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            return await _repo.DeleteAsync(id);
        }
    }
}