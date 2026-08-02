using backend.Models.Abstraction;

namespace backend.Models
{
    public class Doctor : Person
    {
        public required string LicenseNumber { get; set; }

        public required string Specialization { get; set; }

        public decimal ConsulationFee { get; set; }

        public decimal Salary { get; set; }

        public Status status { get; set; } = Status.ACTIVE;

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }

        public List<Appointment> Appointment { get; set; } = new List<Appointment>();

        public List<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();

    }
}
