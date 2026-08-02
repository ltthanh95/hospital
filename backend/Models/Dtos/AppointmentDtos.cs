using System.ComponentModel.DataAnnotations;

namespace backend.Models.Dtos
{
    public class CreateAppointmentRequest
    {
        // Only one of these is required depending on the caller's role:
        // a PATIENT supplies DoctorId (PatientId is derived from their own account),
        // a DOCTOR supplies PatientId (DoctorId is derived from their own account).
        public int? PatientId { get; set; }
        public int? DoctorId { get; set; }

        [Required]
        public required DateTime Schedule { get; set; }

        [Required]
        public required string Reason { get; set; }
    }

    public class RescheduleAppointmentRequest
    {
        [Required]
        public required DateTime Schedule { get; set; }
    }

    public class AppointmentResponse
    {
        public required int Id { get; set; }
        public required DateTime Schedule { get; set; }
        public required AppointmentStatus Status { get; set; }
        public required string Reason { get; set; }
        public required int DoctorId { get; set; }
        public required string DoctorName { get; set; }
        public required int PatientId { get; set; }
        public required string PatientName { get; set; }
        public int? MedicalRecordId { get; set; }

        public static AppointmentResponse FromEntity(Appointment appointment) => new()
        {
            Id = appointment.Id,
            Schedule = appointment.Schedule,
            Status = appointment.Status,
            Reason = appointment.Reason,
            DoctorId = appointment.DoctorId,
            DoctorName = $"{appointment.Doctor.FName} {appointment.Doctor.LName}",
            PatientId = appointment.PatientId,
            PatientName = $"{appointment.Patient.FName} {appointment.Patient.LName}",
            MedicalRecordId = appointment.MedicalRecord?.Id,
        };
    }
}
