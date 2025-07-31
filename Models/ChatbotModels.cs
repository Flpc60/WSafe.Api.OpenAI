namespace WSafe.Api.OpenAI.Models
{
    public class ChatbotRequest
    {
        public string Prompt { get; set; } = string.Empty;
    }

    public class ChatbotResponse
    {
        public string Response { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
