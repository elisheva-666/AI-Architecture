using ChineseAuction.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace ChineseAuction.Api.Models
{
    public enum Role
    {
        Admin,
        Purchaser
    }
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public Role Role { get; set; } = Role.Purchaser;

        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}


