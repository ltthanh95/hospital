using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class ChatSession
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int PatientId { get; set; }
        public Patient Patient { get; set; } = null!;

        public int? DoctorId { get; set; }
        public Doctor? Doctor { get; set; }

        public ChatMode Mode { get; set; } = ChatMode.BOT;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ClosedAt { get; set; }

        public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}
