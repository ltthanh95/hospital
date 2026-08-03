using backend.dbContext;
using backend.Models;
using backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories
{
    public class ChatSessionRepository : Repository<ChatSession>, IChatSessionRepository
    {
        public ChatSessionRepository(ApplicationDbContext db) : base(db)
        {
        }

        public new async Task<IEnumerable<ChatSession>> GetAllAsync()
        {
            return await _dbSet
                .Include(session => session.Patient)
                .Include(session => session.Doctor)
                .Include(session => session.Messages)
                .ToListAsync();
        }

        public new async Task<ChatSession?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(session => session.Patient)
                .Include(session => session.Doctor)
                .Include(session => session.Messages)
                .FirstOrDefaultAsync(session => session.Id == id);
        }

        public async Task<ChatSession?> GetMostRecentOpenSessionAsync(int patientId)
        {
            return await _dbSet
                .Include(session => session.Patient)
                .Include(session => session.Doctor)
                .Include(session => session.Messages)
                .Where(session => session.PatientId == patientId && session.ClosedAt == null)
                .OrderByDescending(session => session.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<ChatMessage> AddMessageAsync(int sessionId, ChatSenderRole senderRole, int? senderUserId, string content)
        {
            var message = new ChatMessage
            {
                ChatSessionId = sessionId,
                SenderRole = senderRole,
                SenderUserId = senderUserId,
                Content = content,
            };

            _db.ChatMessages.Add(message);
            await _db.SaveChangesAsync();

            return message;
        }

        public async Task<IEnumerable<ChatSession>> GetForDoctorAsync(int doctorId)
        {
            return await _dbSet
                .Include(session => session.Patient)
                .Include(session => session.Doctor)
                .Include(session => session.Messages)
                .Where(session =>
                    session.ClosedAt == null &&
                    (session.Mode == ChatMode.WAITING_FOR_DOCTOR ||
                     (session.Mode == ChatMode.LIVE && session.DoctorId == doctorId)))
                .OrderByDescending(session => session.CreatedAt)
                .ToListAsync();
        }
    }
}
