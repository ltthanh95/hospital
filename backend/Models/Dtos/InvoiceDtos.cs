using System.ComponentModel.DataAnnotations;

namespace backend.Models.Dtos
{
    public class GenerateInvoiceRequest
    {
        [Required]
        public required int PatientId { get; set; }
    }

    public class InvoiceItemResponse
    {
        public required int Id { get; set; }
        public required string Description { get; set; }
        public required int Quantity { get; set; }
        public required decimal Price { get; set; }
        public required decimal LineTotal { get; set; }

        public static InvoiceItemResponse FromEntity(InvoiceItem item) => new()
        {
            Id = item.Id,
            Description = item.Description,
            Quantity = item.Quantity,
            Price = item.Price,
            LineTotal = item.Quantity * item.Price,
        };
    }

    public class InvoiceResponse
    {
        public required int Id { get; set; }
        public required int PatientId { get; set; }
        public required string PatientName { get; set; }
        public required DateTime IssuedDate { get; set; }
        public required decimal Total { get; set; }
        public required List<InvoiceItemResponse> Items { get; set; }

        public static InvoiceResponse FromEntity(Invoice invoice) => new()
        {
            Id = invoice.Id,
            PatientId = invoice.PatientId,
            PatientName = $"{invoice.Patient.FName} {invoice.Patient.LName}",
            IssuedDate = invoice.IssuedDate,
            Total = invoice.Total,
            Items = invoice.InvoiceItems.Select(InvoiceItemResponse.FromEntity).ToList(),
        };
    }
}
