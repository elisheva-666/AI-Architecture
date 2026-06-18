using AutoMapper;
using ChineseAuction.Api.Dtos;
using ChineseAuction.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChineseAuction.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly ILogger<UsersController> _logger;


        public UsersController( IUserService userService, ILogger<UsersController> logger , IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
            _logger = logger;
        }

        //קבלת כל המשתמשים
        [Authorize(Roles = "Admin")]
        [HttpGet("Admine")]
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetAll()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        //קבלת משתמש לפי מזהה
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponseDto>> GetById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);

            if (user == null)
            {
                return NotFound(new { message = $"User with ID {id} not found." });
            }

            return Ok(user);
        }


        //יצירת משתמש חדש
        [HttpPost]
        public async Task<ActionResult<UserResponseDto>> Create([FromBody] UserCreateDto createDto)
        {
            try
            {
                var user = await _userService.CreateUserAsync(createDto);
                return CreatedAtAction(nameof(GetById), user);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        //עדכון משתמש קיים
        //[HttpPut("{id}")]

        //public async Task<ActionResult<UserResponseDto>> Update(int id, [FromBody] UserUpdateDto updateDto)
        //{
        //    try
        //    {
        //        var user = await _userService.UpdateUserAsync(id, updateDto);

        //        if (user == null)
        //        {
        //            return NotFound(new { message = $"User with ID {id} not found." });
        //        }

        //        return Ok(user);
        //    }
        //    catch (ArgumentException ex)
        //    {
        //        return BadRequest(new { message = ex.Message });
        //    }
        //}


        //מחיקת משתמש קיים
        //[HttpDelete("{id}")]

        //[ProducesResponseType(StatusCodes.Status204NoContent)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        //public async Task<IActionResult> Delete(int id)
        //{
        //    var result = await _userService.DeleteUserAsync(id);

        //    if (!result)
        //    {
        //        return NotFound(new { message = $"User with ID {id} not found." });
        //    }

        //    return NoContent();
        //}
    }
}
