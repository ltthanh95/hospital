using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using backend.Models;
using Microsoft.Extensions.Options;

namespace backend.Services.Chat
{
    public class OpenRouterChatBotService : IChatBotService
    {
        private const string SystemPrompt =
            "You are a helpful assistant for patients of a hospital. " +
            "If the patient asks about their own medical records, appointments, or profile, use the " +
            "get_my_medical_records or get_my_patient_profile tools to look up real data before answering — " +
            "never invent medical record details. If the patient asks to book an appointment, gather the " +
            "doctor id, desired date/time, and reason, then call create_appointment. " +
            "If the patient asks a general medical question and no patient-specific data applies, give a brief, " +
            "cautious general-information answer and clearly state you are not a substitute for professional " +
            "medical advice — recommend they confirm with their doctor. Never provide a diagnosis. " +
            "Keep answers concise.";

        private const int MaxToolCallRounds = 3;

        private readonly HttpClient _httpClient;
        private readonly OpenRouterSettings _settings;
        private readonly IChatToolExecutor _toolExecutor;

        public OpenRouterChatBotService(HttpClient httpClient, IOptions<OpenRouterSettings> settings, IChatToolExecutor toolExecutor)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _toolExecutor = toolExecutor;

            if (_httpClient.BaseAddress is null)
            {
                _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
            }

            if (!string.IsNullOrWhiteSpace(_settings.ApiKey))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
            }
        }

        public async Task<string> GetReplyAsync(int patientUserId, IReadOnlyList<ChatMessage> history, CancellationToken cancellationToken)
        {
            var messages = new List<OpenRouterMessage>
            {
                new() { Role = "system", Content = SystemPrompt },
            };

            foreach (var message in history.Where(m => m.SenderRole is ChatSenderRole.PATIENT or ChatSenderRole.BOT))
            {
                messages.Add(new OpenRouterMessage
                {
                    Role = message.SenderRole == ChatSenderRole.PATIENT ? "user" : "assistant",
                    Content = message.Content,
                });
            }

            var tools = BuildToolDefinitions();

            for (var round = 0; round < MaxToolCallRounds; round++)
            {
                var request = new OpenRouterChatRequest
                {
                    Model = _settings.Model,
                    Messages = messages,
                    Tools = tools,
                };

                using var response = await _httpClient.PostAsJsonAsync("chat/completions", request, cancellationToken);
                response.EnsureSuccessStatusCode();

                var body = await response.Content.ReadFromJsonAsync<OpenRouterChatResponse>(cancellationToken: cancellationToken);
                var assistantMessage = body?.Choices?.FirstOrDefault()?.Message;

                if (assistantMessage is null)
                {
                    return "Sorry, I wasn't able to generate a response just now. Please try again.";
                }

                if (assistantMessage.ToolCalls is null || assistantMessage.ToolCalls.Count == 0)
                {
                    return assistantMessage.Content ?? string.Empty;
                }

                messages.Add(assistantMessage);

                foreach (var toolCall in assistantMessage.ToolCalls)
                {
                    var result = await _toolExecutor.ExecuteAsync(
                        patientUserId,
                        toolCall.Function.Name,
                        toolCall.Function.Arguments,
                        cancellationToken);

                    messages.Add(new OpenRouterMessage
                    {
                        Role = "tool",
                        ToolCallId = toolCall.Id,
                        Name = toolCall.Function.Name,
                        Content = result,
                    });
                }
            }

            return "I looked into that but I'm having trouble finishing the request — could you rephrase or try again?";
        }

        private static List<OpenRouterToolDefinition> BuildToolDefinitions() =>
        [
            new OpenRouterToolDefinition
            {
                Function = new OpenRouterFunctionSchema
                {
                    Name = "get_my_medical_records",
                    Description = "Get the current patient's own medical records (diagnosis, notes, visit dates, doctor).",
                    Parameters = new
                    {
                        type = "object",
                        properties = new { },
                    },
                },
            },
            new OpenRouterToolDefinition
            {
                Function = new OpenRouterFunctionSchema
                {
                    Name = "get_my_patient_profile",
                    Description = "Get the current patient's own profile (name, blood type, admission status, appointments).",
                    Parameters = new
                    {
                        type = "object",
                        properties = new { },
                    },
                },
            },
            new OpenRouterToolDefinition
            {
                Function = new OpenRouterFunctionSchema
                {
                    Name = "create_appointment",
                    Description = "Book a new appointment for the current patient with a doctor.",
                    Parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            doctorId = new { type = "integer", description = "The id of the doctor to book with." },
                            schedule = new { type = "string", description = "The desired appointment date/time, ISO 8601." },
                            reason = new { type = "string", description = "The reason for the visit." },
                        },
                        required = new[] { "doctorId", "schedule", "reason" },
                    },
                },
            },
        ];
    }
}
