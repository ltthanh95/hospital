using backend.dbContext;
using backend.Mediator.Interfaces;
using backend.Models;
using backend.Models.Dtos;
using backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Services.Features
{
    public class GetAllPaymentRequest : IRequest<IEnumerable<PaymentResponse>>
    {
    }

    public class GetAllPaymentHandler : IRequestHandler<GetAllPaymentRequest, IEnumerable<PaymentResponse>>
    {
        private readonly IPaymentRepository _paymentRepository;

        public GetAllPaymentHandler(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        public async Task<IEnumerable<PaymentResponse>> HandleAsync(GetAllPaymentRequest request, CancellationToken cancellationToken)
        {
            var payments = await _paymentRepository.GetAllAsync();
            return payments.Select(PaymentResponse.FromEntity);
        }
    }

    public class GetPaymentByIdRequest : IRequest<PaymentResponse>
    {
        public required int PaymentId { get; init; }
    }

    public class GetPaymentByIdHandler : IRequestHandler<GetPaymentByIdRequest, PaymentResponse>
    {
        private readonly IPaymentRepository _paymentRepository;

        public GetPaymentByIdHandler(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        public async Task<PaymentResponse> HandleAsync(GetPaymentByIdRequest request, CancellationToken cancellationToken)
        {
            var payment = await _paymentRepository.GetByIdAsync(request.PaymentId)
                ?? throw new KeyNotFoundException($"Payment {request.PaymentId} not found.");

            return PaymentResponse.FromEntity(payment);
        }
    }

    public class CreatePaymentCommand : IRequest<PaymentResponse>
    {
        public required int InvoiceId { get; init; }
        public required decimal Amount { get; init; }
        public required PaymentMethod Method { get; init; }
    }

    public class CreatePaymentHandler : IRequestHandler<CreatePaymentCommand, PaymentResponse>
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly ApplicationDbContext _db;

        public CreatePaymentHandler(IPaymentRepository paymentRepository, ApplicationDbContext db)
        {
            _paymentRepository = paymentRepository;
            _db = db;
        }

        public async Task<PaymentResponse> HandleAsync(CreatePaymentCommand request, CancellationToken cancellationToken)
        {
            var invoice = await _db.Invoices
                .Include(invoice => invoice.Payments)
                .FirstOrDefaultAsync(invoice => invoice.Id == request.InvoiceId, cancellationToken)
                ?? throw new KeyNotFoundException($"Invoice {request.InvoiceId} not found.");

            var alreadyPaid = invoice.Payments
                .Where(payment => payment.Status == PaymentStatus.COMPLETED)
                .Sum(payment => payment.Amount);
            var remaining = invoice.Total - alreadyPaid;

            if (request.Amount > remaining)
            {
                throw new ArgumentException($"Payment amount ({request.Amount:C}) exceeds the remaining balance ({remaining:C}) on this invoice.");
            }

            var payment = new Payment
            {
                InvoiceId = request.InvoiceId,
                PatientId = invoice.PatientId,
                Amount = request.Amount,
                PaymentDate = DateTime.UtcNow,
                Method = request.Method,
                Status = PaymentStatus.COMPLETED,
            };

            await _paymentRepository.AddAsync(payment);
            await _paymentRepository.SaveChangesAsync();

            var created = await _paymentRepository.GetByIdAsync(payment.Id)
                ?? throw new KeyNotFoundException("Payment not found after creation.");

            return PaymentResponse.FromEntity(created);
        }
    }
}
