using ChineseAuction.Api.Dtos;

namespace ChineseAuction.Api.Services
{
    public interface IUserService
    {
        Task<LoginResponseDto> CreateUserAsync(UserCreateDto createDto);
        Task<IEnumerable<UserResponseDto>> GetAllUsersAsync();
        Task<UserResponseDto?> GetUserByIdAsync(int id);
        Task<LoginResponseDto?> AuthenticateAsync(string email, string password);
    }
}