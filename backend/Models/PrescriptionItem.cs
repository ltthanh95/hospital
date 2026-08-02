using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class PrescriptionItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int MedicineId { get; set; }

        public Medicine Medicine { get; set; } = null!;

        public int PrescriptionId { get; set; }

        public Prescription Prescription { get; set; } = null!;

        public required string Dosage { get; set; }

        public int Quantity { get; set; }

        public required string Frequency { get; set; }

        public int durationDays { get; set; }
    }
}
