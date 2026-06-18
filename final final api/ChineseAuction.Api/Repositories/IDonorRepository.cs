//using ChineseAuction.Api.Data;
//using ChineseAuction.Api.Dtos;
//using ChineseAuction.Api.Models;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Logging;

using ChineseAuction.Api.Models;

namespace ChineseAuction.Api.Repositories
{
    public interface IDonorRepository
    {
        Task<int> CreateAsync(Donor donor);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsByEmailAsync(string email, int? excludeId = null);
        Task<IEnumerable<Donor>> GetAllAsync();
        Task<IEnumerable<Donor>> GetByEmailAsync(string email);
        Task<Donor?> GetByIdAsync(int id);
        Task<IEnumerable<Donor>> GetByNameAsync(string name);
        Task UpdateAsync(Donor donor);
    }
}