//using ChineseAuction.Api.Dtos;
//using ChineseAuction.Api.Repositories;
//using Microsoft.Extensions.Logging;
//using System.Collections.Generic;
//using System.Threading.Tasks;

using ChineseAuction.Api.Dtos;

namespace ChineseAuction.Api.Services
{
    public interface IDonorService
    {
        Task<int> CreateAsync(DonorCreateDto dto);
        Task<IEnumerable<DonorDto>> DeleteAsync(int id);
        Task<IEnumerable<DonorDto>> GetAllAsync();
        Task<IEnumerable<DonorDto>> GetByEmailAsync(string email);
        Task<DonorDetailDto?> GetByIdAsync(int id);
        Task<IEnumerable<DonorDto>> GetByNameAsync(string name);
        Task<bool> UpdateAsync(int id, DonorUpdateDto dto);
    }
}