using ChineseAuction.Api.Models;

namespace ChineseAuction.Api.Repositories
{
    public interface IOrderRepository
    {
        Task<Order> CreateAsync(Order order);
        Task<bool> ExistsAsync(int id);
        Task<IEnumerable<Order>> GetAllAsync();
        Task<Order?> GetByIdAsync(int id);
        Task<IEnumerable<Order>> GetByUserIdAsync(int userId);
        Task UpdateAsync(Order order);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<Order>> GetByGiftIdAsync(int giftId);
        Task<bool> DeleteOrderItemAsync(int orderId, int orderItemId);
    }
}