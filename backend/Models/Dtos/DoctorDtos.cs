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

    public class DoctorUpdateDetails
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

    public class UpdateDoctorSalaryBody
    {
        [Range(0, double.MaxValue)]
        public decimal Salary { get; set; }
    }

    public class UpdateDoctorStatusBody : IValidatableObject
    {
        [Required]
        public required Status Status { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Status != Status.ACTIVE && Status != Status.INACTIVE)
            {
                yield return new ValidationResult(
                    "Status must be either ACTIVE or INACTIVE.",
                    [nameof(Status)]);
            }
        }
    }

    public class DoctorAppointmentSummary
    {
        public required int Id { get; set; }
        public required DateTime Schedule { get; set; }
        public required AppointmentStatus Status { get; set; }
        public required string Reason { get; set; }
        public required int PatientId { get; set; }
        public required string PatientName { get; set; }

        public static DoctorAppointmentSummary FromEntity(Appointment appointment) => new()
        {
            Id = appointment.Id,
            Schedule = appointment.Schedule,
            Status = appointment.Status,
            Reason = appointment.Reason,
            PatientId = appointment.PatientId,
            PatientName = $"{appointment.Patient.FName} {appointment.Patient.LName}",
        };
    }

    public class DoctorMedicalRecordSummary
    {
        public required int Id { get; set; }
        public required DateTime Visit { get; set; }
        public required string Diagnosis { get; set; }
        public string? Notes { get; set; }
        public required string PatientName { get; set; }

        public static DoctorMedicalRecordSummary FromEntity(MedicalRecord record) => new()
        {
            Id = record.Id,
            Visit = record.Visit,
            Diagnosis = record.Diagnosis,
            Notes = record.Notes,
            PatientName = $"{record.Patient.FName} {record.Patient.LName}",
        };
    }

    public class DoctorResponse
    {
        public required int Id { get; set; }
        public required string Username { get; set; }
        public required string FName { get; set; }
        public required string LName { get; set; }
        public required DateTime DoB { get; set; }
        public required Gender Gender { get; set; }
        public required string Address { get; set; }
        public required string Phone { get; set; }
        public required string Email { get; set; }
        public required string LicenseNumber { get; set; }
        public required string Specialization { get; set; }
        public required decimal ConsulationFee { get; set; }
        public required decimal Salary { get; set; }
        public required Status Status { get; set; }
        public string? DepartmentName { get; set; }
        public required List<DoctorAppointmentSummary> Appointments { get; set; }
        public required List<DoctorMedicalRecordSummary> MedicalRecords { get; set; }

        public static DoctorResponse FromEntity(Doctor doctor) => new()
        {
            Id = doctor.Id,
            Username = doctor.User.Username,
            FName = doctor.FName,
            LName = doctor.LName,
            DoB = doctor.DoB,
            Gender = doctor.Gender,
            Address = doctor.Address,
            Phone = doctor.Phone,
            Email = doctor.Email,
            LicenseNumber = doctor.LicenseNumber,
            Specialization = doctor.Specialization,
            ConsulationFee = doctor.ConsulationFee,
            Salary = doctor.Salary,
            Status = doctor.status,
            DepartmentName = doctor.Department?.Name,
            Appointments = doctor.Appointment.Select(DoctorAppointmentSummary.FromEntity).ToList(),
            MedicalRecords = doctor.MedicalRecords.Select(DoctorMedicalRecordSummary.FromEntity).ToList(),
        };
    }
}
