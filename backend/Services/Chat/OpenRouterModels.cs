using System.Text.Json.Serialization;

namespace backend.Services.Chat
{
    public class OpenRouterChatRequest
    {
        [JsonPropertyName("model")]
        public required string Model { get; set; }

        [JsonPropertyName("messages")]
        public required List<OpenRouterMessage> Messages { get; set; }

        [JsonPropertyName("tools")]
        public List<OpenRouterToolDefinition>? Tools { get; set; }
    }

    public class OpenRouterMessage
    {
        [JsonPropertyName("role")]
        public required string Role { get; set; }

        [JsonPropertyName("content")]
        public string? Content { get; set; }

        [JsonPropertyName("tool_calls")]
        public List<OpenRouterToolCall>? ToolCalls { get; set; }

        [JsonPropertyName("tool_call_id")]
        public string? ToolCallId { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    public class OpenRouterToolCall
    {
        [JsonPropertyName("id")]
        public required string Id { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; } = "function";

        [JsonPropertyName("function")]
        public required OpenRouterToolCallFunction Function { get; set; }
    }

    public class OpenRouterToolCallFunction
    {
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("arguments")]
        public required string Arguments { get; set; }
    }

    public class OpenRouterToolDefinition
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "function";

        [JsonPropertyName("function")]
        public required OpenRouterFunctionSchema Function { get; set; }
    }

    public class OpenRouterFunctionSchema
    {
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("description")]
        public required string Description { get; set; }

        [JsonPropertyName("parameters")]
        public required object Parameters { get; set; }
    }

    public class OpenRouterChatResponse
    {
        [JsonPropertyName("choices")]
        public List<OpenRouterChoice>? Choices { get; set; }
    }

    public class OpenRouterChoice
    {
        [JsonPropertyName("message")]
        public OpenRouterMessage? Message { get; set; }
    }
}
