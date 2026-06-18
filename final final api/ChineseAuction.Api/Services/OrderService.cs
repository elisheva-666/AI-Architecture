using AutoMapper;
using ChineseAuction.Api.Dtos;
using ChineseAuction.Api.Models;
using ChineseAuction.Api.Repositories;
using System.Text;

namespace ChineseAuction.Api.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepo;
        private readonly IGiftRepository _giftRepo;
        private readonly IUserRepository _userRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<OrderService> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IKafkaProducer _kafka;
        private readonly string _orderTopic;

        public OrderService(
            IOrderRepository orderRepo,
            IGiftRepository giftRepo,
            IUserRepository userRepo,
            IMapper mapper,
            ILogger<OrderService> logger,
            IEmailSender emailSender,
            IKafkaProducer kafka,
            IConfiguration config)
        {
            _orderRepo = orderRepo;
            _giftRepo = giftRepo;
            _userRepo = userRepo;
            _mapper = mapper;
            _logger = logger;
            _emailSender = emailSender;
            _kafka = kafka;
            _orderTopic = config["Kafka:OrderConfirmedTopic"] ?? "order-confirmed";
        }

        // Get all orders
        public async Task<IEnumerable<OrderResponseDto>> GetAllOrdersAsync()
        {
            var orders = await _orderRepo.GetAllAsync();
            return _mapper.Map<IEnumerable<OrderResponseDto>>(orders);
        }


        // קבל הזמנה לפי מזהה הזמנה
        public async Task<OrderResponseDto?> GetOrderByIdAsync(int id)
        {
            var order = await _orderRepo.GetByIdAsync(id);
            return _mapper.Map<OrderResponseDto?>(order);
        }

        //קבל את כל ההזמנות של משתמש לפי מזהה משתמש
        public async Task<IEnumerable<OrderResponseDto>> GetOrdersByUserIdAsync(int userId)
        {
            var orders = await _orderRepo.GetByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<OrderResponseDto>>(orders);
        }

        // קבל את ההזמנה האחרונה שהיא טיוטה עבור משתמש מסוים
        public async Task<OrderResponseDto?> GetLatestDraftOrderAsync(int userId)
        {
            var orders = await _orderRepo.GetByUserIdAsync(userId);

            // מוצאים את ההזמנה האחרונה שהיא עדיין בסטטוס טיוטה
            var latestDraft = orders
                .Where(o => o.Status == Status.IsDraft)
                .OrderByDescending(o => o.OrderDate)
                .FirstOrDefault();

            return _mapper.Map<OrderResponseDto?>(latestDraft);
        }

        //לקבל את כל המשתמשים שקנו כרטיס למתנה מסוימת לפי id  
        public async Task<IEnumerable<GiftPurchasesDto>> GetByGiftIdAsync(int giftId)
        {
            var allOrders = await _orderRepo.GetByGiftIdAsync(giftId);
            var confirmedOrders = allOrders.Where(o => o.Status == Status.IsConfirmed);

            // 2. שימוש ב-LINQ כדי לקבץ את הנתונים לפי מתנה
            var purchases = confirmedOrders
                .SelectMany(o => o.OrderItems) // משטיח את כל הפריטים מכל ההזמנות לרשימה אחת
                .GroupBy(oi => new { oi.GiftId, oi.Gift.Name }) // מקבץ לפי מתנה
                .Select(group => new GiftPurchasesDto
                {
                    GiftId = group.Key.GiftId,
                    GiftName = group.Key.Name,
                    TotalTicketsSold = group.Sum(oi => oi.Quantity),
                    Buyers = _mapper.Map<List<BuyerDto>>(group.ToList())
                })
                .ToList();

            return purchases;

        }

        //
        public async Task<IEnumerable<GiftPurchasesDto>> GetPurchasesByGiftsAsync()
        {
            // 1. שליפת כל ההזמנות המאושרות בלבד (כי טיוטות לא נחשבות רכישה)
            var allOrders = await _orderRepo.GetAllAsync();
            var confirmedOrders = allOrders.Where(o => o.Status == Status.IsConfirmed);

            // 2. שימוש ב-LINQ כדי לקבץ את הנתונים לפי מתנה
            var purchases = confirmedOrders
                .SelectMany(o => o.OrderItems) // משטיח את כל הפריטים מכל ההזמנות לרשימה אחת
                .GroupBy(oi => new { oi.GiftId, oi.Gift.Name }) // מקבץ לפי מתנה
                .Select(group => new GiftPurchasesDto
                {
                    GiftId = group.Key.GiftId,
                    GiftName = group.Key.Name,
                    TotalTicketsSold = group.Sum(oi => oi.Quantity),
                    Buyers = _mapper.Map<List<BuyerDto>>(group.ToList())
                })
                .ToList();

            return purchases;
        }


        //צור הזמנה חדשה
        public async Task<OrderResponseDto> CreateOrderAsync(OrderCreateDto dto)
        {
            //  חיפוש הזמנה פתוחה 
            var allUserOrders = await _orderRepo.GetByUserIdAsync(dto.UserId);
            var openOrder = allUserOrders.FirstOrDefault(o => o.Status == Status.IsDraft);

            if (openOrder != null)
            {
                // --- הוספה להזמנה קיימת ---
                foreach (var itemDto in dto.OrderItems)
                {
                    var existingItem = openOrder.OrderItems.FirstOrDefault(oi => oi.GiftId == itemDto.GiftId);
                    if (existingItem != null)
                        existingItem.Quantity += itemDto.Quantity; // עדכון כמות למתנה קיימת
                    else
                        openOrder.OrderItems.Add(_mapper.Map<OrderItem>(itemDto)); // הוספת מתנה חדשה לסל
                }

                // עדכון סכום כולל
                openOrder.TotalAmount = await CalculateTotal(openOrder.OrderItems);
                await _orderRepo.UpdateAsync(openOrder);
                return _mapper.Map<OrderResponseDto>(await _orderRepo.GetByIdAsync(openOrder.Id));
            }

            // --- לוגיקת יצירת הזמנה חדשה ---
            var newOrder = _mapper.Map<Order>(dto);
            newOrder.Status = Status.IsDraft;
            newOrder.OrderDate = DateTime.Now;
            newOrder.TotalAmount = await CalculateTotal(newOrder.OrderItems);

            var created = await _orderRepo.CreateAsync(newOrder);
            return _mapper.Map<OrderResponseDto>(await _orderRepo.GetByIdAsync(created.Id));
        }

        // חישוב הסכום הכולל של פריטי ההזמנה
        private async Task<decimal> CalculateTotal(IEnumerable<OrderItem> items)
        {
            decimal sum = 0;
            foreach (var item in items)
            {
                var gift = await _giftRepo.GetByIdAsync(item.GiftId);
                if (gift != null) sum += (gift.TicketPrice * item.Quantity);
            }
            return sum;
        }        //אישור הזמנה
        public async Task<bool> ConfirmOrderAsync(int id)
        {
            var order = await _orderRepo.GetByIdAsync(id);
            if (order == null || order.Status == Status.IsConfirmed) return false;

            order.Status = Status.IsConfirmed;
            await _orderRepo.UpdateAsync(order);

            var user = await _userRepo.GetByIdAsync(order.UserId);

            // ── Kafka event ──────────────────────────────────────────────
            try
            {
                var kafkaEvent = new OrderConfirmedEvent
                {
                    OrderId     = order.Id,
                    UserId      = order.UserId,
                    UserName    = user?.Name ?? string.Empty,
                    UserEmail   = user?.Email ?? string.Empty,
                    TotalAmount = order.TotalAmount,
                    ConfirmedAt = DateTime.UtcNow,
                    Items = order.OrderItems.Select(oi => new OrderConfirmedEventItem
                    {
                        GiftId    = oi.GiftId,
                        GiftName  = oi.Gift?.Name ?? $"Gift#{oi.GiftId}",
                        Quantity  = oi.Quantity,
                        UnitPrice = oi.Gift?.TicketPrice ?? 0,
                        LineTotal = (oi.Gift?.TicketPrice ?? 0) * oi.Quantity
                    }).ToList()
                };
                await _kafka.PublishAsync(_orderTopic, order.Id.ToString(), kafkaEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish Kafka event for order #{OrderId}", id);
            }

            // ── Email ─────────────────────────────────────────────────────
            try
            {
                if (user != null && !string.IsNullOrWhiteSpace(user.Email))
                {
                    var subject = $"✅ אישור הזמנה #{order.Id} - Chinese Auction";
                    var body    = BuildOrderConfirmationEmail(order, user);
                    await _emailSender.SendEmailAsync(user.Email, subject, body);
                    _logger.LogInformation("Order confirmation email sent to {Email} for order #{OrderId}", user.Email, order.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send confirmation email for order #{OrderId}", id);
            }

            return true;
        }

        /// <summary>
        /// Builds a detailed order confirmation email body.
        /// </summary>
        private static string BuildOrderConfirmationEmail(Order order, User user)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"שלום {user.Name},");
            sb.AppendLine();
            sb.AppendLine($"הזמנתך #{order.Id} אושרה בהצלחה! 🎉");
            sb.AppendLine($"תאריך הזמנה: {order.OrderDate:dd/MM/yyyy HH:mm}");
            sb.AppendLine();
            sb.AppendLine("פרטי הפריטים שנרכשו:");
            sb.AppendLine("─────────────────────────────────────");

            foreach (var item in order.OrderItems)
            {
                var giftName = item.Gift?.Name ?? $"מתנה #{item.GiftId}";
                var unitPrice = item.Gift?.TicketPrice ?? 0;
                var lineTotal = unitPrice * item.Quantity;
                sb.AppendLine($"  • {giftName}");
                sb.AppendLine($"    כמות כרטיסים: {item.Quantity}  |  מחיר ליחידה: ₪{unitPrice:F2}  |  סה\"כ: ₪{lineTotal:F2}");
            }

            sb.AppendLine("─────────────────────────────────────");
            sb.AppendLine($"סה\"כ לתשלום: ₪{order.TotalAmount:F2}");
            sb.AppendLine();
            sb.AppendLine("בהצלחה בהגרלה! 🍀");
            sb.AppendLine();
            sb.AppendLine("בברכה,");
            sb.AppendLine("צוות Chinese Auction");
            return sb.ToString();
        }


        //מחיקת הזמנה לפי מזהה
        public async Task<bool> DeleteOrderAsync(int orderId, int userId)
        {
            var order = await _orderRepo.GetByIdAsync(orderId);

            if (order == null || order.UserId != userId) return false;

            if (order.Status == Status.IsConfirmed)
            {
                throw new InvalidOperationException("אא למחוק הזמנה לאחר רכישה!");
            }

            return await _orderRepo.DeleteAsync(orderId);
        }

        // מחיקת פריט מהזמנה
        public async Task<bool> DeleteOrderItemAsync(int orderId, int orderItemId, int userId)
        {
            // שליפת ההזמנה ובדיקת בעלות וסטטוס
            var order = await _orderRepo.GetByIdAsync(orderId);

            if (order == null || order.UserId != userId || order.Status != Status.IsDraft)
                return false;

            // מחיקת המוצר מההזמנה
            return await _orderRepo.DeleteOrderItemAsync(orderId, orderItemId);
        }
    }
}
