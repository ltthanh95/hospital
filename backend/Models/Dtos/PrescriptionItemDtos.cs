using System.ComponentModel.DataAnnotations;

namespace backend.Models.Dtos
{
    public class CreatePrescriptionItemRequest
    {
        [Required]
        public required int PrescriptionId { get; set; }

        [Required]
        public required int MedicineId { get; set; }

        [Required]
        public required string Dosage { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        public required string Frequency { get; set; }

        [Range(1, int.MaxValue)]
        public int DurationDays { get; set; }
    }

    public class UpdatePrescriptionItemRequest
    {
        [Required]
        public required int MedicineId { get; set; }

        [Required]
        public required string Dosage { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        public required string Frequency { get; set; }

        [Range(1, int.MaxValue)]
        public int DurationDays { get; set; }
    }

    public class PrescriptionItemResponse
    {
        public required int Id { get; set; }
        public required int PrescriptionId { get; set; }
        public required int MedicineId { get; set; }
        public required string MedicineName { get; set; }
        public required string Dosage { get; set; }
        public required int Quantity { get; set; }
        public required string Frequency { get; set; }
        public required int DurationDays { get; set; }

        public static PrescriptionItemResponse FromEntity(PrescriptionItem item) => new()
        {
            Id = item.Id,
            PrescriptionId = item.PrescriptionId,
            MedicineId = item.MedicineId,
            MedicineName = item.Medicine.Name,
            Dosage = item.Dosage,
            Quantity = item.Quantity,
            Frequency = item.Frequency,
            DurationDays = item.durationDays,
        };
    }
}
