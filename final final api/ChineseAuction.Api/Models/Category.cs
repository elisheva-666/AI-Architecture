using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ChineseAuction.Api.Models
{
    public class Category
    {
        public int Id { get; set; } 

        [Required, MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        public ICollection<Gift> gifts { get; set; } = new List<Gift>(); 
    }
}
