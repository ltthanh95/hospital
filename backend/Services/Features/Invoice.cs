using backend.dbContext;
using backend.Mediator.Interfaces;
using backend.Models;
using backend.Models.Dtos;
using backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Services.Features
{
    public class GetAllInvoiceRequest : IRequest<IEnumerable<InvoiceResponse>>
    {
    }

    public class GetAllInvoiceHandler : IRequestHandler<GetAllInvoiceRequest, IEnumerable<InvoiceResponse>>
    {
        private readonly IInvoiceRepository _invoiceRepository;

        public GetAllInvoiceHandler(IInvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        public async Task<IEnumerable<InvoiceResponse>> HandleAsync(GetAllInvoiceRequest request, CancellationToken cancellationToken)
        {
            var invoices = await _invoiceRepository.GetAllAsync();
            return invoices.Select(InvoiceResponse.FromEntity);
        }
    }

    public class GetMyInvoicesRequest : IRequest<IEnumerable<InvoiceResponse>>
    {
        public required int RequestingUserId { get; init; }
    }

    public class GetMyInvoicesHandler : IRequestHandler<GetMyInvoicesRequest, IEnumerable<InvoiceResponse>>
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IPatientRepository _patientRepository;

        public GetMyInvoicesHandler(IInvoiceRepository invoiceRepository, IPatientRepository patientRepository)
        {
            _invoiceRepository = invoiceRepository;
            _patientRepository = patientRepository;
        }

        public async Task<IEnumerable<InvoiceResponse>> HandleAsync(GetMyInvoicesRequest request, CancellationToken cancellationToken)
        {
            var patient = await _patientRepository.GetByUserIdAsync(request.RequestingUserId)
                ?? throw new KeyNotFoundException("Patient profile not found for the current user.");

            var invoices = await _invoiceRepository.GetAllAsync();
            return invoices.Where(invoice => invoice.PatientId == patient.Id).Select(InvoiceResponse.FromEntity);
        }
    }

    public class GetInvoiceByIdRequest : IRequest<InvoiceResponse>
    {
        public required int InvoiceId { get; init; }
    }

    public class GetInvoiceByIdHandler : IRequestHandler<GetInvoiceByIdRequest, InvoiceResponse>
    {
        private readonly IInvoiceRepository _invoiceRepository;

        public GetInvoiceByIdHandler(IInvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        public async Task<InvoiceResponse> HandleAsync(GetInvoiceByIdRequest request, CancellationToken cancellationToken)
        {
            var invoice = await _invoiceRepository.GetByIdAsync(request.InvoiceId)
                ?? throw new KeyNotFoundException($"Invoice {request.InvoiceId} not found.");

            return InvoiceResponse.FromEntity(invoice);
        }
    }

    public class GenerateInvoiceCommand : IRequest<InvoiceResponse>
    {
        public required int PatientId { get; init; }
    }

    public class GenerateInvoiceHandler : IRequestHandler<GenerateInvoiceCommand, InvoiceResponse>
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly ApplicationDbContext _db;

        public GenerateInvoiceHandler(IInvoiceRepository invoiceRepository, ApplicationDbContext db)
        {
            _invoiceRepository = invoiceRepository;
            _db = db;
        }

        public async Task<InvoiceResponse> HandleAsync(GenerateInvoiceCommand request, CancellationToken cancellationToken)
        {
            var patient = await _db.Patients
                .Include(p => p.Stays).ThenInclude(stay => stay.Room)
                .Include(p => p.MedicalRecords).ThenInclude(record => record.Doctor)
                .Include(p => p.MedicalRecords).ThenInclude(record => record.Prescriptions).ThenInclude(prescription => prescription.PrescriptionItems).ThenInclude(item => item.Medicine)
                .FirstOrDefaultAsync(p => p.Id == request.PatientId, cancellationToken)
                ?? throw new KeyNotFoundException($"Patient {request.PatientId} not found.");

            var items = new List<InvoiceItem>();

            // Room stay charges: only completed stays are billable.
            foreach (var stay in patient.Stays.Where(stay => stay.CheckOut is not null))
            {
                var nights = StayNights.Calculate(stay.CheckIn, stay.CheckOut);
                items.Add(new InvoiceItem
                {
                    Description = $"Room {stay.Room.RoomNumber} stay ({nights} night(s))",
                    Quantity = nights,
                    Price = BillingRates.RoomRatePerNight,
                });
            }

            foreach (var record in patient.MedicalRecords)
            {
                items.Add(new InvoiceItem
                {
                    Description = $"Consultation - Dr. {record.Doctor.FName} {record.Doctor.LName}",
                    Quantity = 1,
                    Price = record.Doctor.ConsulationFee,
                });

                foreach (var prescriptionItem in record.Prescriptions.SelectMany(prescription => prescription.PrescriptionItems))
                {
                    items.Add(new InvoiceItem
                    {
                        Description = $"Medicine - {prescriptionItem.Medicine.Name}",
                        Quantity = prescriptionItem.Quantity,
                        Price = prescriptionItem.Medicine.UnitPrice,
                    });
                }
            }

            if (items.Count == 0)
            {
                throw new InvalidOperationException("No billable charges found for this patient.");
            }

            var invoice = new Invoice
            {
                PatientId = patient.Id,
                IssuedDate = DateTime.UtcNow,
                InvoiceItems = items,
                Total = items.Sum(item => item.Quantity * item.Price),
            };

            await _invoiceRepository.AddAsync(invoice);
            await _invoiceRepository.SaveChangesAsync();

            var created = await _invoiceRepository.GetByIdAsync(invoice.Id)
                ?? throw new KeyNotFoundException("Invoice not found after creation.");

            return InvoiceResponse.FromEntity(created);
        }
    }

    public class DeleteInvoiceRequest : IRequest<bool>
    {
        public required int InvoiceId { get; init; }
    }

    public class DeleteInvoiceHandler : IRequestHandler<DeleteInvoiceRequest, bool>
    {
        private readonly IInvoiceRepository _invoiceRepository;

        public DeleteInvoiceHandler(IInvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        public async Task<bool> HandleAsync(DeleteInvoiceRequest request, CancellationToken cancellationToken)
        {
            var invoice = await _invoiceRepository.GetByIdAsync(request.InvoiceId)
                ?? throw new KeyNotFoundException($"Invoice {request.InvoiceId} not found.");

            _invoiceRepository.Remove(invoice);
            await _invoiceRepository.SaveChangesAsync();

            return true;
        }
    }
}
