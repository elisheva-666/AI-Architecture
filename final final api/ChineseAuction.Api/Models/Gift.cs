using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ChineseAuction.Api.Models
{
    //[Index(nameof(Name))]
    //[Index(nameof(TicketPrice))]
    public class Gift
    {
        public int Id { get; set; } 

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Range(0.1, 10000.00)]
        public decimal TicketPrice { get; set; } 
        public string? ImageUrl { get; set; }

        public int CategoryId { get; set; } 
        public Category? Category { get; set; }

        public int DonorId { get; set; } 
        public Donor? Donor { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>(); 
        public Winner? Winner { get; set; } 
    }
}
