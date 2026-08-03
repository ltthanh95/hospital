namespace backend.Services.Chat
{
    public class OpenRouterSettings
    {
        public required string ApiKey { get; set; }
        public required string Model { get; set; }
        public string BaseUrl { get; set; } = "https://openrouter.ai/api/v1";
    }
}
