//using ChineseAuction.Api.Data;
//using ChineseAuction.Api.Dtos;
//using ChineseAuction.Api.Models;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Logging;

//namespace ChineseAuction.Api.Repositories
//{
//    public class DonorRepository : IDonorRepository
//    {
//        private readonly AppDbContext _context;
//        private readonly ILogger<DonorRepository> _logger;

//        // ctor - מקבל את ה-DbContext ואת ה-Logger (להזרקה ב-Program.cs)
//        public DonorRepository(AppDbContext context, ILogger<DonorRepository> logger)
//        {
//            _context = context;
//            _logger = logger;
//        }

//        /// <summary>
//        /// מחזיר את כל התורמים
//        /// </summary>
//        public async Task<IEnumerable<Donor>> GetAllAsync()
//        {
//            return await _context.Donors.AsNoTracking().ToListAsync();
//        }

//        /// <summary>
//        /// מחזיר פרטי תורם לפי מזהה כולל המתנות שלו
//        /// </summary>
//        public async Task<Donor?> GetByIdAsync(int id)
//        {
//            return await _context.Donors
//                .Include(d => d.Gifts) // טעינת המתנות הקשורות
//                .FirstOrDefaultAsync(d => d.Id == id);
//        }

//        /// <summary>
//        /// מחפש תורמים לפי אימייל (exact, case-insensitive)
//        /// </summary>
//        public async Task<bool> ExistsByEmailAsync(string email, int? excludeId = null)
//        {
//            return await _context.Donors.AnyAsync(d => d.Email == email && d.Id != excludeId);
//        }


//        /// <summary>
//        /// מחפש תורמים לפי שם (חיפוש חלקי, case-insensitive)
//        /// </summary>
//        public async Task<IEnumerable<DonorDto>> GetDonorsByNameAsync(string name)
//        {
//            if (string.IsNullOrWhiteSpace(name)) return Enumerable.Empty<DonorDto>();

//            var normalized = name.Trim().ToLower();
//            return await _context.Donors
//                .AsNoTracking()
//                .Where(d => d.Name != null && d.Name.ToLower().Contains(normalized))
//                .Select(d => new DonorDto
//                {
//                    Id = d.Id,
//                    Name = d.Name ?? string.Empty,
//                    Email = d.Email,
//                    Phone = d.Phone
//                })
//                .ToListAsync();
//        }

//        /// <summary>
//        /// יוצר תורם; במידה ויש מתנות חדשות - יוצר אותן ומקשר לתורם; במידה נמסרו ExistingGiftIds - מקשר אותן לתורם
//        /// מחזיר את ה-id של התורם שנוצר
//        /// </summary>
//        public async Task<int> CreateDonorAsync(DonorCreateDto dto)
//        {
//            try
//            {
//                var name = dto.Name.Trim();
//                if (string.IsNullOrWhiteSpace(name))
//                    throw new ArgumentException("Name is required", nameof(dto.Name));

//                // בדיקה כפילות על בסיס אימייל במידה וסופק
//                if (!string.IsNullOrWhiteSpace(dto.Email))
//                {
//                    var emailNormalized = dto.Email.Trim().ToLower();
//                    var exists = await _context.Donors.AnyAsync(d => d.Email != null && d.Email.ToLower() == emailNormalized);
//                    if (exists)
//                        throw new InvalidOperationException("Donor with the same email already exists.");
//                }

//                var donorEntity = new Donor
//                {
//                    Name = name,
//                    Email = dto.Email?.Trim(),
//                    Phone = dto.Phone?.Trim()
//                };

//                _context.Donors.Add(donorEntity);
//                await _context.SaveChangesAsync(); // need Id for gift linking

//                // קישור מתנות קיימות על ידי id
//                if (dto.ExistingGiftIds != null)
//                {
//                    var existingGifts = await _context.Gifts
//                        .Where(g => dto.ExistingGiftIds.Contains(g.Id))
//                        .ToListAsync();

//                    foreach (var gift in existingGifts)
//                    {
//                        gift.DonorId = donorEntity.Id;
//                    }
//                }

//                // יצירת מתנות חדשות והקשר לתורם
//                if (dto.NewGifts != null)
//                {
//                    var newGiftEntities = dto.NewGifts.Select(g => new Gift
//                    {
//                        Name = g.Name,
//                        Description = g.Description,
//                        TicketPrice = g.TicketPrice,
//                        CategoryId = g.CategoryId ?? 0, // אם אין קטגוריה - ייתכן צורך בהתאמה לפי לוגיקה שלך
//                        DonorId = donorEntity.Id
//                    });

//                    _context.Gifts.AddRange(newGiftEntities);
//                }

//                await _context.SaveChangesAsync();
//                return donorEntity.Id;
//            }
//            catch (DbUpdateException dbEx)
//            {
//                _logger.LogError(dbEx, "Database error while creating donor '{Name}'", dto.Name);
//                throw;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error while creating donor '{Name}'", dto.Name);
//                throw;
//            }
//        }

//        /// <summary>
//        /// מעדכן פרטי תורם (Name, Email, Phone). מחזיר true אם עודכן, false אם לא נמצא.
//        /// </summary>
//        public async Task<bool> UpdateDonorAsync(int id, DonorUpdateDto dto)
//        {
//            try
//            {
//                var existing = await _context.Donors.FindAsync(id);
//                if (existing == null) return false;

//                // בדיקת אימייל כפול (אם שונה)
//                if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email.Trim() != existing.Email)
//                {
//                    var emailNormalized = dto.Email.Trim().ToLower();
//                    var duplicate = await _context.Donors
//                        .AsNoTracking()
//                        .AnyAsync(d => d.Id != id && d.Email != null && d.Email.ToLower() == emailNormalized);
//                    if (duplicate)
//                        throw new InvalidOperationException("Another donor with the same email already exists.");
//                }

//                existing.Name = dto.Name.Trim();
//                existing.Email = dto.Email?.Trim();
//                existing.Phone = dto.Phone?.Trim();

//                await _context.SaveChangesAsync();
//                return true;
//            }
//            catch (DbUpdateException dbEx)
//            {
//                _logger.LogError(dbEx, "Database error while updating donor id {Id}", id);
//                throw;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error while updating donor id {Id}", id);
//                throw;
//            }
//        }

//        /// <summary>
//        /// מוחק תורם לפי מזהה ומחזיר את רשימת התורמים המעודכנת.
//        /// </summary>
//        public async Task<IEnumerable<DonorDto>> DeleteDonorAsync(int id)
//        {
//            try
//            {
//                var donor = await _context.Donors.FindAsync(id);
//                if (donor == null)
//                {
//                    // אם לא נמצא - מחזיר את הרשימה הנוכחית ללא שינוי
//                    return await GetAllDonorsAsync();
//                }

//                // אופציונלי: במידה וקיימות מתנות שקשורות, החלטה עסקית: למחוק/להשאיר/לנקות DonorId
//                // כאן נסתיר את המתנות ונשאיר אותן עם DonorId = null (אם שדה DonorId יכול להיות nullable)
//                // אך במודל שלך DonorId נראה לא-nullable; כדי לפשט - נמחק את המתנות יחד עם התורם
//                var gifts = await _context.Gifts.Where(g => g.DonorId == id).ToListAsync();
//                if (gifts.Any())
//                {
//                    _context.Gifts.RemoveRange(gifts);
//                }

//                _context.Donors.Remove(donor);
//                await _context.SaveChangesAsync();

//                return await GetAllDonorsAsync();
//            }
//            catch (DbUpdateException dbEx)
//            {
//                _logger.LogError(dbEx, "Database error while deleting donor id {Id}", id);
//                throw;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error while deleting donor id {Id}", id);
//                throw;
//            }
//        }
//    }
//}




using ChineseAuction.Api.Data;
using ChineseAuction.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ChineseAuction.Api.Repositories
{
    public class DonorRepository : IDonorRepository
    {
        private readonly AppDbContext _context;

        public DonorRepository(AppDbContext context)
        {
            _context = context;
        }

        // מחזיר את כל התורמים 
        public async Task<IEnumerable<Donor>> GetAllAsync()
        {
            return await _context.Donors
                .AsNoTracking() 
                .ToListAsync();
        }

        // מחזיר תורם ספציפי כולל רשימת המתנות המקושרות אליו (Eager Loading)
        public async Task<Donor?> GetByIdAsync(int id)
        {
            return await _context.Donors
                .Include(d => d.Gifts)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        //חיפוש לפי מייל 
        public async Task<IEnumerable<Donor>> GetByEmailAsync(string email)
        {
            return await _context.Donors
                .Where(d => d.Email != null && d.Email.Contains(email))
                .ToListAsync();
        }

        // חיפוש לפי שם
        public async Task<IEnumerable<Donor>> GetByNameAsync(string name)
        {
            return await _context.Donors
               .Where(d => d.Name != null && d.Name.Contains(name))
                .ToListAsync();
        }

        // הוספת תורם חדש
        public async Task<int> CreateAsync(Donor donor)
        {
            _context.Donors.Add(donor);
            await _context.SaveChangesAsync(); // זה מה ששומר ב-SQL!
            return donor.Id; // מחזיר את ה-ID שנוצר
        }



        // עדכון תורם קיים
        public async Task UpdateAsync(Donor donor)
        {
            _context.Donors.Update(donor);
            await _context.SaveChangesAsync();
        }

        // מחיקת תורם לפי מזהה
        public async Task<bool> DeleteAsync(int id)
        {
            var donor = await _context.Donors.FindAsync(id);
            if (donor == null) return false;

            _context.Donors.Remove(donor);
            await _context.SaveChangesAsync();
            return true;
        }

        // בדיקה האם קיים אימייל במערכת (עם אופציה להחריג מזהה מסוים בעדכון)
        public async Task<bool> ExistsByEmailAsync(string email, int? excludeId = null)
        {
            return await _context.Donors
                .AnyAsync(d => d.Email == email && d.Id != excludeId);
        }
    }
}