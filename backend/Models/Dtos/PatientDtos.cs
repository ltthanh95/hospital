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

    public class AppointmentSummary
    {
        public required int Id { get; set; }
        public required DateTime Schedule { get; set; }
        public required Status Status { get; set; }
        public required string Reason { get; set; }
        public required string DoctorName { get; set; }

        public static AppointmentSummary FromEntity(Appointment appointment) => new()
        {
            Id = appointment.Id,
            Schedule = appointment.Schedule,
            Status = appointment.Status,
            Reason = appointment.Reason,
            DoctorName = $"{appointment.Doctor.FName} {appointment.Doctor.LName}",
        };
    }

    public class MedicalRecordSummary
    {
        public required int Id { get; set; }
        public required DateTime Visit { get; set; }
        public required string Diagnosis { get; set; }
        public string? Notes { get; set; }
        public required string DoctorName { get; set; }

        public static MedicalRecordSummary FromEntity(MedicalRecord record) => new()
        {
            Id = record.Id,
            Visit = record.Visit,
            Diagnosis = record.Diagnosis,
            Notes = record.Notes,
            DoctorName = $"{record.Doctor.FName} {record.Doctor.LName}",
        };
    }

    public class PatientResponse
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
        public required string BloodType { get; set; }
        public required string EmergencyContact { get; set; }
        public required DateTime AdmissionDate { get; set; }
        public DateTime? DischargeDate { get; set; }
        public required Status Status { get; set; }
        public required List<AppointmentSummary> Appointments { get; set; }
        public required List<MedicalRecordSummary> MedicalRecords { get; set; }

        public static PatientResponse FromEntity(Patient patient) => new()
        {
            Id = patient.Id,
            Username = patient.User.Username,
            FName = patient.FName,
            LName = patient.LName,
            DoB = patient.DoB,
            Gender = patient.Gender,
            Address = patient.Address,
            Phone = patient.Phone,
            Email = patient.Email,
            BloodType = patient.BloodType,
            EmergencyContact = patient.EmergencyContact,
            AdmissionDate = patient.AdmissionDate,
            DischargeDate = patient.DischargeDate,
            Status = patient.status,
            Appointments = patient.Appointment.Select(AppointmentSummary.FromEntity).ToList(),
            MedicalRecords = patient.MedicalRecords.Select(MedicalRecordSummary.FromEntity).ToList(),
        };
    }
}
