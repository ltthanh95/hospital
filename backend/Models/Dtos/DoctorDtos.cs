using System.ComponentModel.DataAnnotations;

namespace backend.Models.Dtos
{
    public class DoctorRegistrationDetails
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
        public required string LicenseNumber { get; set; }

        [Required]
        public required string Specialization { get; set; }

        [Range(0, double.MaxValue)]
        public decimal ConsulationFee { get; set; }

        [Required]
        public required string DepartmentName { get; set; }
    }
}
