using ChineseAuction.Api.Data;
using ChineseAuction.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ChineseAuction.Api.Repositories
{
    public class LotteryRepository : ILotteryRepository
    {
        private readonly AppDbContext _context;

        public LotteryRepository(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// שומר את ה-Winner לטבלה ומחזיר את האוביקט שנשמר
        /// </summary>
        public async Task<Winner> SaveWinnerAsync(Winner winner)
        {
            _context.Winners.Add(winner);
            await _context.SaveChangesAsync();
            return winner;
        }

        /// <summary>
        /// מחזיר את כל הרשומות של הזוכים מהמסד
        /// </summary>
        public async Task<IEnumerable<Winner>> GetAllWinnersAsync()
        {
            return await _context.Winners
                .Include(w => w.Gift)
                .Include(w => w.User)
                .ToListAsync();
        }

        /// <summary>
        /// מחשב את הסכום הכולל של ההכנסות מכל ההזמנות המאושרות
        /// </summary>
        public async Task<decimal> GetTotalRevenueAsync()
        {
            // סכום כל ה-TotalAmount של הזמנות עם סטטוס IsConfirmed
            var total = await _context.Orders
                .Where(o => o.Status == Status.IsConfirmed)
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;

            return total;
        }
    }
}