using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class InvoiceItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public required string Description { get; set; }

        public int Quantity { get; set; }

        public decimal Price { get; set; }

        public int InvoiceId { get; set; }
        public Invoice Invoice { get; set; } = null!;
    }
}
