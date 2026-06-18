//using ChineseAuction.Api.Models;
//using System.ComponentModel.DataAnnotations;

//namespace ChineseAuction.Api.Dtos
//{
//    public class UserCreateDto
//    {
//        [Required]
//        [MaxLength(100)]
//        public string Name { get; set; } = string.Empty;

//        [Required]
//        [EmailAddress]
//        [MaxLength(200)]
//        public string Email { get; set; } = string.Empty;

//        [Required]
//        [MinLength(4, ErrorMessage = "Password must be at least 4 characters")]
//        public string Password { get; set; } = string.Empty;

//        [Phone]
//        [MaxLength(20)]
//        public string Phone { get; set; } = string.Empty;

//    }

//    public class UserUpdateDto
//    {
//        [MaxLength(100)]
//        public string? Name { get; set; }

//        [EmailAddress]
//        [MaxLength(200)]
//        public string? Email { get; set; }

//        [Phone]
//        [MaxLength(20)]
//        public string? Phone { get; set; }

//    }

//    public class UserResponseDto
//    {
//        public string Name { get; set; } = string.Empty;
//        public string Email { get; set; } = string.Empty;
//        public string Phone { get; set; } = string.Empty;
//        public Role Role { get; set; } 
//    }
//}



using ChineseAuction.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace ChineseAuction.Api.Dtos
{
    // ---------------------------
    // Authentication DTOs
    // ---------------------------

    // For user registration
    public class RegisterDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = null!;

        [Required, EmailAddress]
        public string Email { get; set; } = null!;

        [Required, Phone]
        public string Phone { get; set; } = null!;

        [Required, MinLength(6)]
        public string Password { get; set; } = null!;
    }

    // For login requests
    public class LoginRequestDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;
    }

    // For login responses (token + user info)
    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string TokenType { get; set; } = "Bearer";
        public int ExpiresIn { get; set; } // seconds
        public UserResponseDto User { get; set; } = null!;
    }

    // ---------------------------
    // User Management DTOs
    // ---------------------------

    public class UserCreateDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(4)]
        public string Password { get; set; } = string.Empty;

        [Phone, MaxLength(20)]
        public string Phone { get; set; } = string.Empty;
    }


    public class UserUpdateDto
    {
        [MaxLength(100)]
        public string? Name { get; set; }

        [EmailAddress, MaxLength(200)]
        public string? Email { get; set; }

        [Phone, MaxLength(20)]
        public string? Phone { get; set; }
    }

    public class UserResponseDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; 
    }

    // ---------------------------
    // Auth Token DTO
    // ---------------------------

    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public string Role { get; set; } = string.Empty;
    }
}

