using ChineseAuction.Api.Dtos;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ChineseAuction.Api.Services
{
    public interface IAiService
    {
        Task<string> GetAnswerAsync(string userQuestion);
    }

    public class OpenAiService : IAiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public OpenAiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<string> GetAnswerAsync(string userQuestion)
        {
            var apiKey = _configuration["OpenAi:ApiKey"];
            var model = _configuration["OpenAi:Model"] ?? "gpt-3.5-turbo";

            // 1. הגדרת האישיות והחוקים (System Prompt)
            var systemMessage = @"
                        את עוזרת אישית בשם 'אלישבע' באתר מכירה סינית.
                        התפקיד שלך: לסייע למשתמשים במידע על מתנות, מחירים ורכישה.
                        אתה יכול לענות על כל השאלות לא דווקא שקשורות למכירה, אלא גם על שאלות כלליות על האתר,
                        תהליך הרכישה, מדיניות החזרה וכו'.
                        סגנון דיבור: אינטליגנטי (כמו Gemini), אדיב, סבלני וקליל.

                        //חוקים קריטיים:
                        //1. אל תעני על שום נושא דתי, הלכתי או תורני.
                        //2. אם שואלים על נושא אסור (חוסר צניעות, אלימות וכו'), עני רק: 'אופסס..., נסחפנו לגמרי אתה לא ברמה הזאת https://netfree.link'.
                        3. אם המשתמש שואל על מתנה ספציפית, בדקי בהקשר שסופק לך אם היא קיימת ועני עליה.
                        ";

            // 2. בניית הבקשה
            var requestBody = new OpenAiRequest
            {
                model = model,
                messages = new List<OpenAiMessage>
                {
                    new OpenAiMessage { role = "system", content = systemMessage },
                    new OpenAiMessage { role = "user", content = userQuestion }
                }
            };

            // 3. שליחה ל-OpenAI
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json");

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);

            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", jsonContent);

            if (!response.IsSuccessStatusCode)
            {
                return "מצטערת, יש כרגע עומס במערכת. נסה שוב מאוחר יותר.";
            }

            var responseString = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<OpenAiResponse>(responseString);

            // 4. החזרת הטקסט בלבד
            return result?.choices?.FirstOrDefault()?.message?.content
                   ?? "לא התקבלה תשובה ברורה.";
        }
    }
}