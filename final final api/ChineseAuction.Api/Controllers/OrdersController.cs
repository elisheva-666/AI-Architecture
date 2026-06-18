using ChineseAuction.Api.Dtos;
using ChineseAuction.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChineseAuction.Api.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {

        private readonly IOrderService _orderService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        /// <summary>
        /// עבור מנהל: צפייה בכל הרכישות המאושרות בלבד
        /// </summary>
        [HttpGet("Admine")]
        [Authorize(Roles = "Admin")] // רק מנהל יכול לגשת
        public async Task<ActionResult<IEnumerable<OrderResponseDto>>> GetAllConfirmed()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            var confirmedOrders = orders.Where(o => o.Status == "IsConfirmed");
            return Ok(confirmedOrders);
        }


        /// <summary>
        /// עבור מנהל: צפייה ברכישות כרטיסים עבור כל מתנה
        /// </summary>
        [HttpGet("Admine/by-gifts")]
        [Authorize(Roles = "Admin")] // הגנה קשיחה - רק למנהל
        public async Task<ActionResult<IEnumerable<GiftPurchasesDto>>> GetPurchasesByGifts()
        {
            var report = await _orderService.GetPurchasesByGiftsAsync();
            return Ok(report);
        }

        /// <summary>
        ///  לפי הid של המתנה --עבור מנהל: צפייה ברכישות כרטיסים עבור כל מתנה
        /// </summary>
        [HttpGet("Admine/by-gifts-id")]
        //[Authorize(Roles = "Admin")] // הגנה קשיחה - רק למנהל
        public async Task<ActionResult<IEnumerable<GiftPurchasesDto>>> GetByGiftIdAsync(int giftId)
        {
            var report = await _orderService.GetByGiftIdAsync(giftId);
            return Ok(report);
        }


        /// <summary>
        /// עבור לקוח: צפייה בסל האישי שלו (טיוטות בלבד)
        /// </summary>
        [HttpGet("my-cart")]
        public async Task<ActionResult<OrderResponseDto>> GetMyCart()
        {
            var userId = GetUserIdFromToken();
            // שינוי כאן: אנחנו קוראים למתודה שמחזירה אובייקט בודד
            var cart = await _orderService.GetLatestDraftOrderAsync(userId);

            if (cart == null) return NotFound(new { message = "העגלה ריקה" });

            return Ok(cart);
        }


        //לקבל הזמנה לפי מזהה
        //[HttpGet("{id}")]
        //public async Task<ActionResult<OrderResponseDto>> GetById(int id)
        //{
        //    var order = await _orderService.GetOrderByIdAsync(id);

        //    if (order == null)
        //    {
        //        return NotFound(new { message = $"Order with ID {id} not found." });
        //    }

        //    return Ok(order);
        //}

        ////לקבל הזמנות לפי מזהה משתמש
        //[HttpGet("user/{userId}")]
        //public async Task<ActionResult<IEnumerable<OrderResponseDto>>> GetByUserId(int userId)
        //{
        //    var orders = await _orderService.GetOrdersByUserIdAsync(userId);
        //    return Ok(orders);
        //}


        /// <summary>
        /// יצירת הזמנה חדשה או עדכון סל קיים
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<OrderResponseDto>> Create(List<OrderItemCreateDto> createItemsDto)
        {
            var createDto = new OrderCreateDto
            {
                OrderItems = createItemsDto,
                UserId = GetUserIdFromToken()
            };
            var result = await _orderService.CreateOrderAsync(createDto);
            return Ok(result);

        }


        /// <summary>
        /// אישור רכישה: הפיכת טיוטה להזמנה סופית
        /// </summary>
        [HttpPut("{id}/confirm")]
        public async Task<IActionResult> Confirm(int id)
        {
            var userId = GetUserIdFromToken();
            var order = await _orderService.GetOrderByIdAsync(id);

            if (order == null || order.UserId != userId)
                throw new KeyNotFoundException("הזמנה לא נמצאה");

            var success = await _orderService.ConfirmOrderAsync(id);
            if (!success)
                throw new InvalidOperationException("לא ניתן לאשר את ההזמנה");

            return Ok(new { message = "הרכישה בוצעה בהצלחה!" });
        }

        /// <summary>
        /// מחיקת טיוטה בלבד - רק בעל ההזמנה יכול למחוק
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var userId = GetUserIdFromToken();
                // ה-Service כבר בודק אם זו טיוטה ואם היא שייכת למשתמש
                var success = await _orderService.DeleteOrderAsync(id, userId);

                if (!success) return NotFound("הזמנה לא נמצאה או שאינה שייכת לך");

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                // כאן תיתפס השגיאה במידה וההזמנה כבר אושרה
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// מחיקת מוצר מסל הקניות (רק כאשר ההזמנה בסטטוס טיוטה)
        /// </summary>
        [HttpDelete("{orderId}/items/{orderItemId}")]
        public async Task<IActionResult> DeleteOrderItem(int orderId, int orderItemId)
        {
            var userId = GetUserIdFromToken();
            var success = await _orderService.DeleteOrderItemAsync(orderId, orderItemId, userId);

            if (!success)
                return NotFound(new { message = "המוצר לא נמצא בהזמנה או שההזמנה אינה בסטטוס טיוטה" });

            return NoContent();
        }

        // פונקציית עזר לשליפת ה-ID מהטוקן
        private int GetUserIdFromToken()
        {
            var claim = User.FindFirst("id") ?? User.FindFirst(ClaimTypes.NameIdentifier);
            return int.Parse(claim?.Value ?? "0");
        }
    }
}

