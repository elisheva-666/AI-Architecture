using ChineseAuction.Api.Data;
using ChineseAuction.Api.Dtos;
using ChineseAuction.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ChineseAuction.Api.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CategoryRepository> _logger;

        public CategoryRepository(AppDbContext context, ILogger<CategoryRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// מחזיר את כל הקטגוריות (קריאה בלבד)
        /// </summary>
        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _context.Categories.AsNoTracking().ToListAsync();
        }

        /// <summary>
        /// מחזיר קטגוריה לפי id או null אם לא קיימת
        /// </summary>
        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _context.Categories.FindAsync(id);
        }

        /// <summary>
        /// יוצר קטגוריה חדשה ומחזיר את ה-id שלה
        /// </summary>
        public async Task<Category> CreateAsync(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        /// <summary>
        /// מעדכן שם של קטגוריה קיימת; מחזיר true אם עודכן, false אם לא נמצא
        /// </summary>
        public async Task UpdateAsync(Category category)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// מוחק קטגוריה לפי id; מחזיר true אם נמחק
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return false;
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }

        // בדיקה מהירה אם קיים שם (עבור ה-Service)
        public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
        {
            return await _context.Categories
                .AnyAsync(c => c.Name.ToLower() == name.ToLower() && c.Id != excludeId);
        }
    }
}
