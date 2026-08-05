using System.ComponentModel.DataAnnotations;

namespace backend.Models.Dtos
{
    public class CreatePrescriptionRequest
    {
        [Required]
        public required int MedicalRecordId { get; set; }

        [Required]
        public required DateTime IssueDate { get; set; }

        [Required]
        public required string Instruction { get; set; }
    }

    public class UpdatePrescriptionRequest
    {
        [Required]
        public required DateTime IssueDate { get; set; }

        [Required]
        public required string Instruction { get; set; }
    }

    public class PrescriptionItemSummary
    {
        public required int Id { get; set; }
        public required int MedicineId { get; set; }
        public required string MedicineName { get; set; }
        public required string Dosage { get; set; }
        public required int Quantity { get; set; }
        public required string Frequency { get; set; }
        public required int DurationDays { get; set; }

        public static PrescriptionItemSummary FromEntity(PrescriptionItem item) => new()
        {
            Id = item.Id,
            MedicineId = item.MedicineId,
            MedicineName = item.Medicine.Name,
            Dosage = item.Dosage,
            Quantity = item.Quantity,
            Frequency = item.Frequency,
            DurationDays = item.durationDays,
        };
    }

    public class PrescriptionResponse
    {
        public required int Id { get; set; }
        public required int MedicalRecordId { get; set; }
        public required DateTime IssueDate { get; set; }
        public required string Instruction { get; set; }
        public required List<int> PrescriptionItemIds { get; set; }
        public required List<PrescriptionItemSummary> Items { get; set; }

        public static PrescriptionResponse FromEntity(Prescription prescription) => new()
        {
            Id = prescription.Id,
            MedicalRecordId = prescription.MedicalRecordId,
            IssueDate = prescription.IssueDate,
            Instruction = prescription.Instruction,
            PrescriptionItemIds = prescription.PrescriptionItems.Select(item => item.Id).ToList(),
            Items = prescription.PrescriptionItems.Select(PrescriptionItemSummary.FromEntity).ToList(),
        };
    }
}
