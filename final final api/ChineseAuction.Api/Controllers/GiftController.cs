using ChineseAuction.Api.Data;
using ChineseAuction.Api.Dtos;
using ChineseAuction.Api.Models;
using ChineseAuction.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChineseAuction.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GiftController : ControllerBase
    {
        private readonly IGiftService _service;
        private readonly IFileService _fileService;
        private readonly AppDbContext _context;



        public GiftController(IGiftService service, IFileService fileService, AppDbContext context)
        {
            _service = service;
            _fileService = fileService;
            _context = context;
        }

        //מחזיר את כל המתנות הזמינות לרוכשים 
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GiftDetailDto>>> GetAll()
            => Ok(await _service.GetAllForBuyersAsync());

        //מיון מתנות לפי מחיר כרטיס
        [HttpGet("sort-by-price")]
        public async Task<ActionResult<IEnumerable<GiftDetailDto>>> GetByPrice([FromQuery] bool asc = true)
            => Ok(await _service.GetAllSortedByPriceAsync(asc));

        //מיון מתנות לפי קטגוריה
        [HttpGet("sort-by-category")]
        public async Task<ActionResult<IEnumerable<GiftDetailDto>>> GetByCategory()
            => Ok(await _service.GetAllSortedByCategoryAsync());

        //מחזיר מתנה לפי Id עם כל הפרטים
        [HttpGet("{id:int}")]
        public async Task<ActionResult<GiftDetailDto>> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            return result == null ? NotFound() : Ok(result);
        }

        //חיפוש מתנות לפי שם, תורם או מינימום רכישות
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<GiftDto>>> Search([FromQuery] string? name, [FromQuery] string? donor, [FromQuery] int? min)
        {
            var results = await _service.SearchAsync(name, donor, min);
            return Ok(results);
        }
        // --- גישה למנהלים בלבד (חובה טוקן עם Role=Admin) ---

        //צפייה מנהלית מורחבת כולל נתוני מכירות ותורמים
        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<GiftAdminDto>>> GetAllForAdmin()
            => Ok(await _service.GetAllForAdminAsync());


        // הוספת מתנה חדשה לתורם קיים
        [HttpPost("admin/add-to-donor/{donorId:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<int>> AddToDonor(int donorId, [FromForm] GiftCreateUpdateDto dto)
        {
            try
            {
                string? imagePath = null;

                if (dto.ImageUrl != null)
                {
                    imagePath = _fileService.SaveFile(dto.ImageUrl);
                }

                var id = await _service.AddToDonorAsync(donorId, dto, imagePath);

                return CreatedAtAction(nameof(GetById), new { id }, id);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }


        // עדכון מתנה קיימת - כולל תמיכה בתמונות
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromForm] GiftCreateUpdateDto dto) // שינוי ל-[FromForm]
        {
            try
            {
                string? imagePath = null;

                // אם המשתמש העלה תמונה חדשה בעדכון
                if (dto.ImageUrl != null)
                {
                    imagePath = _fileService.SaveFile(dto.ImageUrl);
                }

                // שליחה לסרוויס עם נתיב התמונה החדש (אם יש)
                var success = await _service.UpdateAsync(id, dto, imagePath);

                return success ? NoContent() : NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // מחיקת מתנה לפי Id
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _service.DeleteAsync(id);
            return success ? NoContent() : NotFound();
        }


        [HttpPost("admin/add-image")]
        [Authorize(Roles = "Admin")] // הגנה על ה-Action [cite: 13]
        public async Task<IActionResult> AddGift([FromForm] GiftCreateUpdateDto giftDto)
        {
            // 1. הופכים את הקובץ ל-string של נתיב
            string imagePath = _fileService.SaveFile(giftDto.ImageUrl);

            // 2. יוצרים את האובייקט לשמירה ב-DB (כאן ה-ImageUrl הוא string!)
            var gift = new Gift
            {
                Name = giftDto.Name,
                TicketPrice = giftDto.TicketPrice,
                ImageUrl = imagePath, // ה-string נשמר כאן
                Category = giftDto.CategoryId.HasValue ? new Category { Id = giftDto.CategoryId.Value } : null,

            };

            _context.Gifts.Add(gift);
            await _context.SaveChangesAsync();
            return Ok(gift);
        }
    }

}