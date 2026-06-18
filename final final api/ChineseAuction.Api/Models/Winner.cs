namespace ChineseAuction.Api.Models
{
    public class Winner
    {
        public int Id { get; set; } // ID זוכה

        public int GiftId { get; set; } // קשר למתנה: איזו מתנה זכה
        public Gift Gift { get; set; } = null!; // קשר למתנה: איזו מתנה זכה

        public int UserId { get; set; } // קשר לרוכש: מי הזוכה במתנה
        public User User { get; set; } = null!;// קשר לרוכש: מי הזוכה במתנה
    }
}
