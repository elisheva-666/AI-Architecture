namespace ChineseAuction.Api.Dtos
{
    public class ChatRequestDto
    {
        public string UserMessage { get; set; }
    }

    public class ChatResponseDto
    {
        public string BotReply { get; set; }
    }
}
namespace ChineseAuction.Api.Dtos
{
    // מבנה הבקשה שנשלח ל-OpenAI
    public class OpenAiRequest
    {
        public string model { get; set; }
        public List<OpenAiMessage> messages { get; set; }
        public double temperature { get; set; } = 0.7; // יצירתיות
    }

    public class OpenAiMessage
    {
        public string role { get; set; } // "system", "user", "assistant"
        public string content { get; set; }
    }

    // מבנה התשובה שנקבל מ-OpenAI
    public class OpenAiResponse
    {
        public List<Choice> choices { get; set; }
    }

    public class Choice
    {
        public OpenAiMessage message { get; set; }
    }
}