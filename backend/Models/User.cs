using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "The Username field is mandatory.")]
        public required string Username { get; set; }
        [Required(ErrorMessage = "The PasswordHash field is mandatory.")]
        public required string PasswordHash { get; set; }

        public required Role Role { get; set; }

        public Patient? Patient { get; set; }

        public Doctor? Doctor { get; set; }

    }
}
