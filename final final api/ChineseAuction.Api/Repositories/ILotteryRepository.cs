using ChineseAuction.Api.Models;

namespace ChineseAuction.Api.Repositories
{
    public interface ILotteryRepository
    {
        // שומר זוכה לטבלה
        Task<Winner> SaveWinnerAsync(Winner winner);

        // מחזיר את כל הזוכים
        Task<IEnumerable<Winner>> GetAllWinnersAsync();

        // מחשב את כלל ההכנסות מכל ההזמנות המאושרות
        Task<decimal> GetTotalRevenueAsync();
    }
}