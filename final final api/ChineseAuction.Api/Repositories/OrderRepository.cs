using ChineseAuction.Api.Data;
using ChineseAuction.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ChineseAuction.Api.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;

        public OrderRepository(AppDbContext context)
        {
            _context = context;
        }

        //עבור מנהל
        //קבל את כל ההזמנות
        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Gift)
                .ToListAsync();
        }


        //עבור מנהל או משתמש
        //קבל את כל ההזמנות של משתמש לפי מזהה הזמנה
        public async Task<Order?> GetByIdAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Gift)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        //עבור מנהל או משתמש
        //קבל את כל ההזמנות של משתמש לפי מזהה הזמנה
        public async Task<IEnumerable<Order>> GetByUserIdAsync(int userId)
        {
            return await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Gift)
                .ToListAsync();
        }


        //קבל הזמנה לכל מתנה את כל הכרטיסים שרכשוו עבורה
        public async Task<IEnumerable<Order>> GetByGiftIdAsync(int giftId)
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Gift)
                .Where(o => o.OrderItems.Any(oi => oi.GiftId == giftId))
                .ToListAsync();
        }



        //עבור משתמש
        //צור הזמנה חדשה
        public async Task<Order> CreateAsync(Order order)
        {
            order.OrderDate = DateTime.UtcNow;

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return order;
        }

        //עבור משתמש
        //עדכן הזמנה קיימת
        public async Task UpdateAsync(Order order)
        {
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
        }

        //עבור משתמש
        //מחק הזמנה לפי id
        public async Task<bool> DeleteAsync(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return false;

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return true;
        }

        //מחק פריט הזמנה לפי מזהה הזמנה ומזהה פריט הזמנה
        public async Task<bool> DeleteOrderItemAsync(int orderId, int orderItemId)
        {
            var orderItem = await _context.OrderItems
                .FirstOrDefaultAsync(oi => oi.OrderId == orderId && oi.Id == orderItemId);

            if (orderItem == null)
                return false;

            _context.OrderItems.Remove(orderItem);
            await _context.SaveChangesAsync();
            return true;
        }

        //בדוק אם הזמנה קיימת לפי id
        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Orders.AnyAsync(o => o.Id == id);
        }
    }
}
