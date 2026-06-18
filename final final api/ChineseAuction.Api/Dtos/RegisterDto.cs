//using ChineseAuction.Api.Models;
//using System.ComponentModel.DataAnnotations;

//namespace ChineseAuction.Api.Dtos
//{
//    // DTO for user registration
//    public class RegisterDto
//    {
//        [Required, MaxLength(100)]
//        public string Name { get; set; } = null!;

//        [Required, EmailAddress]
//        public string Email { get; set; } = null!;

//        [Required, Phone]
//        public string Phone { get; set; } = null!;

//        [Required, MinLength(6)]
//        public string Password { get; set; } = null!;

//        // optional: allow admin registration only via server config
//        public string Role { get; set; }
//    }

//    // DTO for login
//    public class LoginDto
//    {
//        [Required, EmailAddress]
//        public string Email { get; set; } = null!;

//        [Required]
//        public string Password { get; set; } = null!;
//    }

//    // Token response
//    public class AuthResponseDto
//    {
//        public string Token { get; set; } = string.Empty;
//        public DateTime ExpiresAt { get; set; }
//        public Role Role { get; set; } 
//    }
//}