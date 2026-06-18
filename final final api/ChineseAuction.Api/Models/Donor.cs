using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ChineseAuction.Api.Models
{
    //[Index(nameof(Email), IsUnique = true)]
    public class Donor
    {
        public int Id { get; set; } // ID תורם

        [Required, MaxLength(100)]
        public string? Name { get; set; } // שם

        [Required, EmailAddress]
        public string? Email { get; set; } // מייל - לצורך סינון 

        [Required, Phone]
        public string? Phone { get; set; } // טלפון
        public ICollection<Gift> Gifts { get; set; } = new List<Gift>(); // קשר למתנות: לכל תורם יש רשימת מתנות


    }
}

