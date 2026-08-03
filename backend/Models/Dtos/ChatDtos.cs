namespace backend.Models.Dtos
{
    public class ChatMessageResponse
    {
        public required int Id { get; set; }
        public required ChatSenderRole SenderRole { get; set; }
        public int? SenderUserId { get; set; }
        public required string Content { get; set; }
        public required DateTime SentAt { get; set; }

        public static ChatMessageResponse FromEntity(ChatMessage message) => new()
        {
            Id = message.Id,
            SenderRole = message.SenderRole,
            SenderUserId = message.SenderUserId,
            Content = message.Content,
            SentAt = message.SentAt,
        };
    }

    public class ChatSessionResponse
    {
        public required int Id { get; set; }
        public required int PatientId { get; set; }
        public required string PatientName { get; set; }
        public int? DoctorId { get; set; }
        public string? DoctorName { get; set; }
        public required ChatMode Mode { get; set; }
        public required DateTime CreatedAt { get; set; }
        public required List<ChatMessageResponse> Messages { get; set; }

        public static ChatSessionResponse FromEntity(ChatSession session) => new()
        {
            Id = session.Id,
            PatientId = session.PatientId,
            PatientName = $"{session.Patient.FName} {session.Patient.LName}",
            DoctorId = session.DoctorId,
            DoctorName = session.Doctor is null ? null : $"{session.Doctor.FName} {session.Doctor.LName}",
            Mode = session.Mode,
            CreatedAt = session.CreatedAt,
            Messages = session.Messages.OrderBy(message => message.SentAt).Select(ChatMessageResponse.FromEntity).ToList(),
        };
    }
}
