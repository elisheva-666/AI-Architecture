
using ChineseAuction.Api.Dtos;
using ChineseAuction.Api.Models;
using ChineseAuction.Api.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ChineseAuction.Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _repo;
        private readonly PasswordHasher<User> _hasher = new();
        private readonly IConfiguration _config;

        private readonly string _key;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _expiryMinutes;

        public AuthService(IUserRepository repo, IConfiguration config)
        {
            _repo = repo;
            _config = config;

            _key = _config["Jwt:Key"]
                ?? throw new InvalidOperationException("Jwt:Key missing");

            _issuer = _config["Jwt:Issuer"]
                ?? throw new InvalidOperationException("Jwt:Issuer missing");

            _audience = _config["Jwt:Audience"]
                ?? throw new InvalidOperationException("Jwt:Audience missing");

            _expiryMinutes = int.Parse(_config["Jwt:ExpiryMinutes"] ?? "60");
        }

        //הרשמה
        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            if (await _repo.GetByEmailAsync(dto.Email) != null)
                throw new InvalidOperationException("Email already exists");

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone,
                Role = Role.Purchaser
            };

            user.PasswordHash = _hasher.HashPassword(user, dto.Password);

            await _repo.CreateAsync(user);

            return CreateAuthResponse(user);
        }

        //התחברות
        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto dto)
        {
            var user = await _repo.GetByEmailAsync(dto.Email)
                ?? throw new UnauthorizedAccessException();

            if (_hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password)
                == PasswordVerificationResult.Failed)
                throw new UnauthorizedAccessException();

            return CreateAuthResponse(user);
        }

        //
        private AuthResponseDto CreateAuthResponse(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
            var token = new JwtSecurityToken(
                _issuer,
                _audience,
                claims,
                expires: DateTime.UtcNow.AddMinutes(_expiryMinutes),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            return new AuthResponseDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                ExpiresAt = token.ValidTo,
                Role = user.Role.ToString()
            };
        }
    }
}
