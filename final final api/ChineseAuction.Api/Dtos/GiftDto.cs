using System.ComponentModel.DataAnnotations;

namespace ChineseAuction.Api.Dtos
{
    // מחלקה בסיסית לשדות משותפים (Base)
    public class GiftBaseDto
    {
        public string Name { get; set; } = null!;
        public decimal TicketPrice { get; set; }
    }

    // DTO להצגת מתנה פשוטה (למשל ברשימה לרוכשים)
    public class GiftDto : GiftBaseDto
    {
        public int Id { get; set; }
    }

    // DTO ליצירה ועדכון - כאן יש את הוולידציות
    public class GiftCreateUpdateDto : GiftBaseDto
    {
        [Required, MaxLength(200)]
        public new string Name { get; set; } = null!; // דריסת השדה לצורך וולידציה

        public string? Description { get; set; }

        [Range(0.1, 10000.0)]
        public new decimal TicketPrice { get; set; }

        public IFormFile? ImageUrl { get; set; }
        public int? CategoryId { get; set; }
       // public int? DonorId { get; set; }
    }
  

    // DTO מפורט - יורש מ-GiftDto ומוסיף פרטי תורם וקטגוריה
    public class GiftDetailDto : GiftDto
    {
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public int? DonorId { get; set; }
        public string? DonorName { get; set; }
       
    }

    // DTO למנהל - מוסיף רק את מה שצריך לניהול (כמו כמות רוכשים)
    public class GiftAdminDto : GiftDetailDto

    {
        public string? DonorEmail { get; set; }
        public int PurchasersCount { get; set; }
    }

    public class GiftPurchasesDto
    {
        public int GiftId { get; set; }
        public string GiftName { get; set; } = string.Empty;
        public int TotalTicketsSold { get; set; } // סך כל הכרטיסים שנמכרו למתנה זו
        public List<BuyerDto> Buyers { get; set; } = new();
    }

    public class BuyerDto 
    {
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int Quantity { get; set; } // כמה כרטיסים המשתמש הספציפי קנה
    }


}