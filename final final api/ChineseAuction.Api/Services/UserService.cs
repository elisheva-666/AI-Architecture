using AutoMapper;
using ChineseAuction.Api.Dtos;
using ChineseAuction.Api.Models;
using ChineseAuction.Api.Repositories;

namespace ChineseAuction.Api.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserService> _logger;
        private readonly IMapper _mapper;

        public UserService(
            IUserRepository userRepository,
            ITokenService tokenService,
            IConfiguration configuration,
            ILogger<UserService> logger, IMapper mapper)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _configuration = configuration;
            _logger = logger;
            _mapper = mapper;
        }

        // קבלת כל המשתמשים
        public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<UserResponseDto>>(users);
        }

        // קבלת משתמש לפי מזהה
        public async Task<UserResponseDto?> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            return user == null ? null : _mapper.Map<UserResponseDto>(user);
        }

        // יצירת משתמש חדש — מחזיר token כמו login
        public async Task<LoginResponseDto> CreateUserAsync(UserCreateDto createDto)
        {
            if (await _userRepository.EmailExistsAsync(createDto.Email))
                throw new ArgumentException($"Email {createDto.Email} is already registered.");

            var user = _mapper.Map<User>(createDto);
            user.PasswordHash = HashPassword(createDto.Password);

            var createdUser = await _userRepository.CreateAsync(user);

            var token = _tokenService.GenerateToken(createdUser.Id, createdUser.Email, createdUser.Name, createdUser.Role);
            var expiryMinutes = _configuration.GetValue<int>("Jwt:ExpiryMinutes", 60);

            return new LoginResponseDto
            {
                Token = token,
                TokenType = "Bearer",
                ExpiresIn = expiryMinutes * 60,
                User = _mapper.Map<UserResponseDto>(createdUser)
            };
        }


        public async Task<LoginResponseDto?> AuthenticateAsync(string email, string password)
        {
            var user = await _userRepository.GetByEmailAsync(email);

            if (user == null)
            {
                _logger.LogWarning("Login attempt failed: User not found for email {Email}", email);
                return null;
            }

            // Verify password (simplified - in production use proper password verification)
            var hashedPassword = HashPassword(password);
            if (user.PasswordHash != hashedPassword)
            {
                _logger.LogWarning("Login attempt failed: Invalid password for email {Email}", email);
                return null;
            }

            var token = _tokenService.GenerateToken(user.Id, user.Email, user.Name , user.Role);
            var expiryMinutes = _configuration.GetValue<int>("JwtSettings:ExpiryMinutes", 60);

            _logger.LogInformation("User {UserId} authenticated successfully", user.Id);

            return new LoginResponseDto
            {
                Token = token,
                TokenType = "Bearer",
                ExpiresIn = expiryMinutes * 60,
                User = _mapper.Map<UserResponseDto>(user)
            };
        }



        // Simplified password hashing - In production, use BCrypt, Argon2, or Identity framework
        private static string HashPassword(string password)
        {
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
        }
    }
}
