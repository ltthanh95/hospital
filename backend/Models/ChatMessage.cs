using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class ChatMessage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int ChatSessionId { get; set; }
        public ChatSession ChatSession { get; set; } = null!;

        public ChatSenderRole SenderRole { get; set; }

        public int? SenderUserId { get; set; }

        public required string Content { get; set; }

        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}
