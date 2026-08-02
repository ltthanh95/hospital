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

    public class PrescriptionResponse
    {
        public required int Id { get; set; }
        public required int MedicalRecordId { get; set; }
        public required DateTime IssueDate { get; set; }
        public required string Instruction { get; set; }
        public required List<int> PrescriptionItemIds { get; set; }

        public static PrescriptionResponse FromEntity(Prescription prescription) => new()
        {
            Id = prescription.Id,
            MedicalRecordId = prescription.MedicalRecordId,
            IssueDate = prescription.IssueDate,
            Instruction = prescription.Instruction,
            PrescriptionItemIds = prescription.PrescriptionItems.Select(item => item.Id).ToList(),
        };
    }
}
