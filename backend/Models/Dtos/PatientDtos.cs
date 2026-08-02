using System.ComponentModel.DataAnnotations;

namespace backend.Models.Dtos
{
    public class PatientRegistrationDetails
    {
        [Required]
        public required string FName { get; set; }

        [Required]
        public required string LName { get; set; }

        [Required]
        public required DateTime DoB { get; set; }

        public Gender Gender { get; set; }

        [Required]
        public required string Address { get; set; }

        [Required]
        public required string Phone { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string BloodType { get; set; }

        [Required]
        public required string EmergencyContact { get; set; }
    }
}
