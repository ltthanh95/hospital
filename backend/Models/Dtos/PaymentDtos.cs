using System.ComponentModel.DataAnnotations;

namespace backend.Models.Dtos
{
    public class CreatePaymentRequest
    {
        [Required]
        public required int InvoiceId { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        public required PaymentMethod Method { get; set; }
    }

    public class PaymentResponse
    {
        public required int Id { get; set; }
        public required int InvoiceId { get; set; }
        public required int PatientId { get; set; }
        public required string PatientName { get; set; }
        public required decimal Amount { get; set; }
        public required DateTime PaymentDate { get; set; }
        public required PaymentMethod Method { get; set; }
        public required PaymentStatus Status { get; set; }

        public static PaymentResponse FromEntity(Payment payment) => new()
        {
            Id = payment.Id,
            InvoiceId = payment.InvoiceId,
            PatientId = payment.PatientId,
            PatientName = $"{payment.Patient.FName} {payment.Patient.LName}",
            Amount = payment.Amount,
            PaymentDate = payment.PaymentDate,
            Method = payment.Method,
            Status = payment.Status,
        };
    }
}
