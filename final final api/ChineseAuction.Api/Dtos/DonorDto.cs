using ChineseAuction.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace ChineseAuction.Api.Dtos
{
    // DTO להצגת רשימת תורמים / תצוגה קצרה
    public class DonorDto
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = null!;

        [Required ,EmailAddress]
        public string Email { get; set; } = null!;

        [Required, Phone]
        public string Phone { get; set; } = null!;
    }

    // DTO להצגת פרטי תורם כולל מתנות שהביא
    public class DonorDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Email { get; set; }
        public string? Phone { get; set; }

        // רשימת מתנות מקושרות לתורם
        public IEnumerable<GiftDto> Gifts { get; set; } = new List<GiftDto>();
    }

    // DTO ליצירת תורם. ניתן לספק מזהי מתנות קיימות להקשרה או לציין מתנות חדשות ליצירה.
    public class DonorCreateDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = null!;

        [Required, EmailAddress]
        public string Email { get; set; } = null!;

        [Phone]
        public string? Phone { get; set; }
        public IEnumerable<GiftCreateUpdateDto>? NewGifts { get; set; }
    }


    // DTO לעדכון תורם
    public class DonorUpdateDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = null!;

        [EmailAddress]
        public string? Email { get; set; }

        [Phone]
        public string? Phone { get; set; }
    }
}
