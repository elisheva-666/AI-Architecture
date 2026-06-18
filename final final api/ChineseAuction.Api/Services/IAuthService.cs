//using ChineseAuction.Api.Dtos;
//using ChineseAuction.Api.Models;
//using ChineseAuction.Api.Repositories;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.Extensions.Configuration;
//using Microsoft.IdentityModel.Tokens;
//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;
//using System.Text;

using ChineseAuction.Api.Dtos;

namespace ChineseAuction.Api.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginRequestDto dto);
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    }
}