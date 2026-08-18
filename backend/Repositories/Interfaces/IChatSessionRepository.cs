using backend.Models;

//Template for Chat with the all methods from IRepository
namespace backend.Repositories.Interfaces
{
    public interface IChatSessionRepository : IRepository<ChatSession>
    {
        Task<ChatSession?> GetMostRecentOpenSessionAsync(int patientId);
        Task<IEnumerable<ChatSession>> GetForDoctorAsync(int doctorId);
        Task<ChatMessage> AddMessageAsync(int sessionId, ChatSenderRole senderRole, int? senderUserId, string content);
    }
}
