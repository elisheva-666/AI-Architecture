namespace ChineseAuction.Api.Models
{
    public enum Status
    {
       IsDraft,
       IsConfirmed
    }

    public class Order
    {
        public int Id { get; set; } // ID הזמנה
        public DateTime OrderDate { get; set; } // תאריך ביצוע ההזמנה
        public Status Status { get; set; } = Status.IsDraft;//  נשמר כטיוטה ורק לאחר אישור נרכש בפועל 
        public decimal TotalAmount { get; set; } // הסכום הכולל של ההזמנה
        public int UserId { get; set; } // קשר לרוכש: מי ביצע את ההזמנה
        public User User { get; set; } = null!;// קשר לרוכש: מי ביצע את ההזמנה

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>(); // קשר לפרטי הזמנה – אילו פריטים כלולים בהזמנה זו
    }
}
