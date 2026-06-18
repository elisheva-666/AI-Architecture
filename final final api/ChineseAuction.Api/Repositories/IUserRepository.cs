//using ChineseAuction.Api.Data;
//using ChineseAuction.Api.Models;
//using Microsoft.EntityFrameworkCore;

using ChineseAuction.Api.Models;

namespace ChineseAuction.Api.Repositories
{
    public interface IUserRepository
    {
        Task<User> CreateAsync(User user);
        Task<bool> ExistsAsync(int id);
        Task<IEnumerable<User>> GetAllAsync();
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByIdAsync(int id);
        Task<bool> EmailExistsAsync(string email);
    }
}