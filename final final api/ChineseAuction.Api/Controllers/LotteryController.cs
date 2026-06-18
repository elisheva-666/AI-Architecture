using ChineseAuction.Api.Dtos;
using ChineseAuction.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChineseAuction.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LotteryController : ControllerBase
    {
        private readonly ILotteryService _lotteryService;
        private readonly ILogger<LotteryController> _logger;

        public LotteryController(ILotteryService lotteryService, ILogger<LotteryController> logger)
        {
            _lotteryService = lotteryService;
            _logger = logger;
        }

        // Admin-only: draw single gift
        [HttpPost("draw/{giftId:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<WinnerResultDto>> DrawForGift(int giftId)
        {
            try
            {
                var result = await _lotteryService.DrawForGiftAsync(giftId);
                if (result == null) return NotFound(new { message = "No tickets sold for this gift or winner generation failed." });
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Draw failed for gift {GiftId}", giftId);
                return StatusCode(500, "Draw failed");
            }
        }

        // Admin-only: draw for all gifts and create reports
        [HttpPost("draw-all")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<WinnerResultDto>>> DrawAll()
        {
            try
            {
                var results = await _lotteryService.DrawAllAsync();
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Draw-all failed");
                return StatusCode(500, "Draw-all failed");
            }
        }
    }
}