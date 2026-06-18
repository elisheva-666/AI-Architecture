

using ChineseAuction.Api.Dtos;
using ChineseAuction.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChineseAuction.Api.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class DonorController : ControllerBase
    {
        private readonly IDonorService _service;

        public DonorController(IDonorService service)
        {
            _service = service;
        }

        /// <summary>
        /// שליפת רשימת כל התורמים (תצוגה מקוצרת)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DonorDto>>> GetAll()
        {
            var list = await _service.GetAllAsync();
            return Ok(list);
        }

        /// <summary>
        /// שליפת תורם ספציפי כולל רשימת המתנות שלו
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<DonorDetailDto>> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            return result == null ? NotFound(new { message = $"Donor with ID {id} not found" }) : Ok(result);
        }

        /// <summary>
        /// חיפוש תורמים לפי אימייל
        /// </summary>
        [HttpGet("search/email")]
        public async Task<ActionResult<IEnumerable<DonorDto>>> GetByEmail([FromQuery] string email)
            => Ok(await _service.GetByEmailAsync(email));

        /// <summary>
        /// חיפוש תורמים לפי שם
        /// </summary>
        [HttpGet("search/name")]
        public async Task<ActionResult<IEnumerable<DonorDto>>> GetByName([FromQuery] string name)
            => Ok(await _service.GetByNameAsync(name));


        /// <summary>
        /// יצירת תורם חדש במערכת
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<int>> Create([FromBody] DonorCreateDto dto)
        {
            try
            {
                var id = await _service.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id }, id);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }
     

        /// <summary>
        /// עדכון פרטי תורם קיים
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] DonorUpdateDto dto)
        {
            try
            {
                var success = await _service.UpdateAsync(id, dto);
                return success ? NoContent() : NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        /// <summary>
        /// מחיקת תורם מהמערכת והחזרת הרשימה המעודכנת
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<IEnumerable<DonorDto>>> Delete(int id)
            => Ok(await _service.DeleteAsync(id));
    }
}
