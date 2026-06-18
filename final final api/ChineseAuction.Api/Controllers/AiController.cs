using ChineseAuction.Api.Dtos;
using ChineseAuction.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChineseAuction.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AiController : ControllerBase
    {
        private readonly IAiService _aiService;
        private readonly IGiftService _giftService;

        public AiController(IAiService aiService, IGiftService giftService)
        {
            _aiService = aiService;
            _giftService = giftService;
        }

        [HttpPost("ask")]
        public async Task<ActionResult<ChatResponseDto>> Ask([FromBody] ChatRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.UserMessage))
                return BadRequest(new { message = "הודעה ריקה" });

            try
            {
                // 1. שולפים את המתנות מהמערכת שלך
                var gifts = await _giftService.GetAllForBuyersAsync();
                var giftContext = string.Join("\n", gifts.Select(g => $"- {g.Name}: מחיר כרטיס {g.TicketPrice} ש\"ח."));

                // 2. בונים הנחיית מערכת חזקה (זה מה שהופך אותה לחכמה)
                string systemInstruction = $@"
                את אלישבע, העוזרת האישית של אתר המכירה הסינית. 
                הנה רשימת המתנות שקיימות אצלנו באתר כרגע:
                {giftContext}

                הוראות חשובות:
                - תמיד תהיי אדיבה, מקצועית וקלילה.
                - אם שואלים אותך על מתנה, תבדקי ברשימה למעלה אם היא קיימת ותעני לפי זה.
                - אל תמציאי מתנות שלא קיימות ברשימה.
                - תעני תמיד בעברית בלבד.
               ";

                // 3. שליחה ל-AI עם ההנחיות והשאלה של המשתמש
                string fullPrompt = $"{systemInstruction}\n\nשאלת המשתמש: {request.UserMessage}";

                var answer = await _aiService.GetAnswerAsync(fullPrompt);

                return Ok(new ChatResponseDto { BotReply = answer });
            }
            catch (Exception ex)
            {
                return Ok(new ChatResponseDto { BotReply = "היי, אני כאן! נראה שיש לי קצת קשיי תקשורת עם המוח שלי. אפשר לנסות לשאול שוב?" });
            }
        }
    }
}