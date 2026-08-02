using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Invoice
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public DateTime IssuedDate { get; set; }

        public int PatientId { get; set; }
        public Patient Patient { get; set; } = null!;

        public List<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();

        public List<Payment> Payments { get; set; } = new List<Payment>();

        public decimal Total { get; set; }

    }
}
