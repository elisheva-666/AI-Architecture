using ChineseAuction.Api.Dtos;
using ChineseAuction.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChineseAuction.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IUserService userService, IConfiguration config, ILogger<AuthController> logger)
        {
            _userService = userService;
            _config = config;
            _logger = logger;
        }

        /// <summary>Login — sets an HTTP-only cookie with the JWT token</summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<UserResponseDto>> Login([FromBody] LoginRequestDto loginDto)
        {
            var result = await _userService.AuthenticateAsync(loginDto.Email, loginDto.Password);

            if (result == null)
                return Unauthorized(new { message = "Invalid email or password." });

            SetAuthCookie(result.Token);

            _logger.LogInformation("User {Email} logged in", loginDto.Email);

            // מחזירים רק את פרטי המשתמש — ה-token יושב בcookie, לא ב-body
            return Ok(result.User);
        }

        /// <summary>Register — creates user and sets HTTP-only cookie with JWT token</summary>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<UserResponseDto>> Register([FromBody] UserCreateDto createDto)
        {
            try
            {
                var result = await _userService.CreateUserAsync(createDto);

                SetAuthCookie(result.Token);

                _logger.LogInformation("New user registered: {Email}", createDto.Email);

                return Ok(result.User);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>Logout — deletes the auth cookie</summary>
        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("auth_token", new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Strict
            });

            return Ok(new { message = "Logged out successfully" });
        }

        // ─── helper ──────────────────────────────────────────────────────────
        private void SetAuthCookie(string token)
        {
            var expiryMinutes = _config.GetValue<int>("Jwt:ExpiryMinutes", 60);

            Response.Cookies.Append("auth_token", token, new CookieOptions
            {
                HttpOnly = true,                          // JavaScript לא יכול לקרוא — מגן מפני XSS
                Secure = false,                           // שנה ל-true בproduction (HTTPS)
                SameSite = SameSiteMode.Strict,           // מגן מפני CSRF
                Expires = DateTimeOffset.UtcNow.AddMinutes(expiryMinutes)
            });
        }
    }
}
