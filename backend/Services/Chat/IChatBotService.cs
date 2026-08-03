using backend.Models;

namespace backend.Services.Chat
{
    public interface IChatBotService
    {
        Task<string> GetReplyAsync(int patientUserId, IReadOnlyList<ChatMessage> history, CancellationToken cancellationToken);
    }
}
