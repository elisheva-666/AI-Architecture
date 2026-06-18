using ChineseAuction.Api.Data;
using ChineseAuction.Api.Dtos;
using ChineseAuction.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChineseAuction.Api.Repositories
{
    public class GiftRepository : IGiftRepository
    {
        private readonly AppDbContext _context;

        public GiftRepository(AppDbContext context)
        {
            _context = context;
        }

        // מחזיר שאילתה בסיסית עם כל הקשרים (עוזר למנוע חזרתיות בקוד)
        private IQueryable<Gift> GetGiftsWithIncludes() =>
         _context.Gifts
             .Include(g => g.Category)
             .Include(g => g.Donor)
             .Include(g => g.OrderItems)
             .AsNoTracking();

        //מחזיר את כל המתנות כולל קטגוריה ותורם
        public async Task<IEnumerable<Gift>> GetAllAsync() => await GetGiftsWithIncludes().ToListAsync();

        // מיון לפי מחיר  
        public async Task<IEnumerable<Gift>> GetAllSortedByPriceAsync(bool ascending)
        {
            var query = GetGiftsWithIncludes();

            query = ascending
                ? query.OrderBy(g => g.TicketPrice)
                : query.OrderByDescending(g => g.TicketPrice);

            return await query.ToListAsync();
        }

        // מיון לפי קטגוריה 
        public async Task<IEnumerable<Gift>> GetAllSortedByCategoryAsync()
        {
            return await GetGiftsWithIncludes()
                 .OrderBy(g => g.Category != null ? g.Category.Name : string.Empty) // בדיקה שהקטגוריה לא נול
                 .ToListAsync();
        }


        //מחפש מתנה לפי Id כולל קטגוריה ותורם
        public async Task<Gift?> GetByIdAsync(int id) =>
         await GetGiftsWithIncludes().FirstOrDefaultAsync(g => g.Id == id);

        //יוצר מתנה חדשה ומחזיר את ה Id שלה
        public async Task<int> CreateAsync(Gift gift)
        {
            _context.Gifts.Add(gift);
            await _context.SaveChangesAsync();
            return gift.Id;
        }

        //מחפש מתנה לפי Id כולל קטגוריה ותורם (עם מעקב לשם עדכון)
        public async Task<Gift?> GetByIdTrackedAsync(int id)
        {
            return await _context.Gifts
                .Include(g => g.Category)
                .Include(g => g.Donor)
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        // מעדכן מתנה קיימת
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }



        // מוחק מתנה לפי Id
        public async Task<bool> DeleteAsync(int id)
        {
            var gift = await _context.Gifts.FindAsync(id);
            if (gift == null) return false;
            _context.Gifts.Remove(gift);
            await _context.SaveChangesAsync();
            return true;
        }

        // פונקציה לבדיקת קיום תורם (לפני הוספת מתנה אליו)
        public async Task<bool> DonorExistsAsync(int donorId) =>
            await _context.Donors.AnyAsync(d => d.Id == donorId);

        /// <summary>ביצוע חיפוש וסינון ברמת בסיס הנתונים (SQL)</summary>
        public async Task<IEnumerable<Gift>> SearchGiftsInternalAsync(string? name, string? donor, int? minPurchasers)
        {
            var result = _context.Gifts.Include(g => g.Category).Include(g => g.Donor).Include(g => g.OrderItems).AsNoTracking();

            if (!string.IsNullOrWhiteSpace(name))
                result = result.Where(g => g.Name.Contains(name));

            if (!string.IsNullOrWhiteSpace(donor))
                result = result.Where(g => g.Donor != null && g.Donor.Name!.Contains(donor));

            if (minPurchasers.HasValue)
                result = result.Where(g => g.OrderItems.Count >= minPurchasers.Value);

            return await result.ToListAsync();
        }


    }

}
