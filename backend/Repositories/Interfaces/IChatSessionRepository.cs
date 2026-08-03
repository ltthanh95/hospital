using backend.Models;

namespace backend.Repositories.Interfaces
{
    public interface IChatSessionRepository : IRepository<ChatSession>
    {
        Task<ChatSession?> GetMostRecentOpenSessionAsync(int patientId);
        Task<IEnumerable<ChatSession>> GetForDoctorAsync(int doctorId);
        Task<ChatMessage> AddMessageAsync(int sessionId, ChatSenderRole senderRole, int? senderUserId, string content);
    }
}
