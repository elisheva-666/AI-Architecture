// ChineseAuction.Api.Dtos/OrderDtos.cs
using System.ComponentModel.DataAnnotations;

namespace ChineseAuction.Api.Dtos
{
    public class OrderCreateDto
    {
        [Required]
        public int UserId { get; set; }

        [MinLength(1, ErrorMessage = "ההזמנה חייבת לכלול לפחות פריט אחד")]
        public List<OrderItemCreateDto> OrderItems { get; set; } = new();
    }

    public class OrderItemCreateDto
    {
        [Required]
        public int GiftId { get; set; }
        //[Range(1, 100)]
        public int Quantity { get; set; } = 1;
    }

    public class OrderResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public List<OrderItemResponseDto> OrderItems { get; set; } = new();
    }

    public class OrderItemResponseDto
    {
        public int Id { get; set; }

        public int giftId { get; set; }
        public string GiftName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; } // הוספנו מחיר ליחידה בנפרד
    }
}