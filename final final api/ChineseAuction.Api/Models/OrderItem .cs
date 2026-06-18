using System.ComponentModel.DataAnnotations;

namespace ChineseAuction.Api.Models
{
    public class OrderItem
    {
        public int Id { get; set; } // ID פריט הזמנה

        public int GiftId { get; set; } // קשר למתנה: איזו מתנה נרכשה
        public Gift Gift { get; set; } = null!;// קשר למתנה: איזו מתנה נרכשה

        public int OrderId { get; set; } // קשר להזמנה: באיזו הזמנה נרכשה המתנה
        public Order Order { get; set; } = null!; // קשר להזמנה: באיזו הזמנה נרכשה המתנה

        [Range(1, 1000)]
        public int Quantity { get; set; } // כמות כרטיסים למתנה זו בהזמנה
    }
}
