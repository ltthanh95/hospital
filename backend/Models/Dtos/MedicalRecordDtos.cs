using System.ComponentModel.DataAnnotations;

namespace backend.Models.Dtos
{
    public class CreateMedicalRecordRequest
    {
        [Required]
        public required int PatientId { get; set; }

        public int? AppointmentId { get; set; }

        [Required]
        public required DateTime Visit { get; set; }

        [Required]
        public required string Diagnosis { get; set; }

        public string? Notes { get; set; }
    }

    public class UpdateMedicalRecordRequest
    {
        [Required]
        public required DateTime Visit { get; set; }

        [Required]
        public required string Diagnosis { get; set; }

        public string? Notes { get; set; }
    }

    public class MedicalRecordResponse
    {
        public required int Id { get; set; }
        public required int DoctorId { get; set; }
        public required string DoctorName { get; set; }
        public required int PatientId { get; set; }
        public required string PatientName { get; set; }
        public int? AppointmentId { get; set; }
        public required DateTime Visit { get; set; }
        public required string Diagnosis { get; set; }
        public string? Notes { get; set; }
        public required List<int> PrescriptionIds { get; set; }

        public static MedicalRecordResponse FromEntity(MedicalRecord record) => new()
        {
            Id = record.Id,
            DoctorId = record.DoctorId,
            DoctorName = $"{record.Doctor.FName} {record.Doctor.LName}",
            PatientId = record.PatientId,
            PatientName = $"{record.Patient.FName} {record.Patient.LName}",
            AppointmentId = record.AppointmentId,
            Visit = record.Visit,
            Diagnosis = record.Diagnosis,
            Notes = record.Notes,
            PrescriptionIds = record.Prescriptions.Select(prescription => prescription.Id).ToList(),
        };
    }
}
