//Template for Doctor Present - sytem will check if Doctor is ready for chat
namespace backend.Services.Chat
{
    public interface IDoctorPresenceTracker
    {
        void MarkConnected(int doctorId, string connectionId);
        void MarkDisconnected(int doctorId, string connectionId);
        void SetAvailable(int doctorId);
        void SetUnavailable(int doctorId);
        bool TryGetAvailableDoctor(out int doctorId);
        void Enqueue(int sessionId);
        bool TryDequeueForDoctor(out int sessionId);
        IReadOnlyCollection<string> GetConnections(int doctorId);
    }
}
