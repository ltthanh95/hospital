using backend.Mediator.Interfaces;
using backend.Models;
using backend.Models.Dtos;
using backend.Repositories.Interfaces;

namespace backend.Services.Features
{
    public class GetAllPrescriptionRequest : IRequest<IEnumerable<PrescriptionResponse>>
    {
    }

    public class GetAllPrescriptionHandler : IRequestHandler<GetAllPrescriptionRequest, IEnumerable<PrescriptionResponse>>
    {
        private readonly IPrescriptionRepository _prescriptionRepository;

        public GetAllPrescriptionHandler(IPrescriptionRepository prescriptionRepository)
        {
            _prescriptionRepository = prescriptionRepository;
        }

        public async Task<IEnumerable<PrescriptionResponse>> HandleAsync(GetAllPrescriptionRequest request, CancellationToken cancellationToken)
        {
            var prescriptions = await _prescriptionRepository.GetAllAsync();
            return prescriptions.Select(PrescriptionResponse.FromEntity);
        }
    }

    public class GetMyPrescriptionsRequest : IRequest<IEnumerable<PrescriptionResponse>>
    {
        public required int RequestingUserId { get; init; }
    }

    public class GetMyPrescriptionsHandler : IRequestHandler<GetMyPrescriptionsRequest, IEnumerable<PrescriptionResponse>>
    {
        private readonly IPrescriptionRepository _prescriptionRepository;
        private readonly IMedicalRecordRepository _medicalRecordRepository;
        private readonly IPatientRepository _patientRepository;

        public GetMyPrescriptionsHandler(
            IPrescriptionRepository prescriptionRepository,
            IMedicalRecordRepository medicalRecordRepository,
            IPatientRepository patientRepository)
        {
            _prescriptionRepository = prescriptionRepository;
            _medicalRecordRepository = medicalRecordRepository;
            _patientRepository = patientRepository;
        }

        public async Task<IEnumerable<PrescriptionResponse>> HandleAsync(GetMyPrescriptionsRequest request, CancellationToken cancellationToken)
        {
            var patient = await _patientRepository.GetByUserIdAsync(request.RequestingUserId)
                ?? throw new KeyNotFoundException("Patient profile not found for the current user.");

            var records = await _medicalRecordRepository.GetAllAsync();
            var myRecordIds = records.Where(record => record.PatientId == patient.Id)
                .Select(record => record.Id)
                .ToHashSet();

            var prescriptions = await _prescriptionRepository.GetAllAsync();
            return prescriptions.Where(prescription => myRecordIds.Contains(prescription.MedicalRecordId))
                .Select(PrescriptionResponse.FromEntity);
        }
    }

    public class GetPrescriptionByIdRequest : IRequest<PrescriptionResponse>
    {
        public required int PrescriptionId { get; init; }
    }

    public class GetPrescriptionByIdHandler : IRequestHandler<GetPrescriptionByIdRequest, PrescriptionResponse>
    {
        private readonly IPrescriptionRepository _prescriptionRepository;

        public GetPrescriptionByIdHandler(IPrescriptionRepository prescriptionRepository)
        {
            _prescriptionRepository = prescriptionRepository;
        }

        public async Task<PrescriptionResponse> HandleAsync(GetPrescriptionByIdRequest request, CancellationToken cancellationToken)
        {
            var prescription = await _prescriptionRepository.GetByIdAsync(request.PrescriptionId)
                ?? throw new KeyNotFoundException($"Prescription {request.PrescriptionId} not found.");

            return PrescriptionResponse.FromEntity(prescription);
        }
    }

    public class CreatePrescriptionCommand : IRequest<PrescriptionResponse>
    {
        public required int MedicalRecordId { get; init; }
        public required DateTime IssueDate { get; init; }
        public required string Instruction { get; init; }
    }

    public class CreatePrescriptionHandler : IRequestHandler<CreatePrescriptionCommand, PrescriptionResponse>
    {
        private readonly IPrescriptionRepository _prescriptionRepository;
        private readonly IMedicalRecordRepository _medicalRecordRepository;

        public CreatePrescriptionHandler(
            IPrescriptionRepository prescriptionRepository,
            IMedicalRecordRepository medicalRecordRepository)
        {
            _prescriptionRepository = prescriptionRepository;
            _medicalRecordRepository = medicalRecordRepository;
        }

        public async Task<PrescriptionResponse> HandleAsync(CreatePrescriptionCommand request, CancellationToken cancellationToken)
        {
            _ = await _medicalRecordRepository.GetByIdAsync(request.MedicalRecordId)
                ?? throw new KeyNotFoundException($"MedicalRecord {request.MedicalRecordId} not found.");

            var prescription = new Prescription
            {
                MedicalRecordId = request.MedicalRecordId,
                IssueDate = request.IssueDate,
                Instruction = request.Instruction,
            };

            await _prescriptionRepository.AddAsync(prescription);
            await _prescriptionRepository.SaveChangesAsync();

            var created = await _prescriptionRepository.GetByIdAsync(prescription.Id)
                ?? throw new KeyNotFoundException("Prescription not found after creation.");

            return PrescriptionResponse.FromEntity(created);
        }
    }

    public class UpdatePrescriptionCommand : IRequest<PrescriptionResponse>
    {
        public required int PrescriptionId { get; init; }
        public required DateTime IssueDate { get; init; }
        public required string Instruction { get; init; }
    }

    public class UpdatePrescriptionHandler : IRequestHandler<UpdatePrescriptionCommand, PrescriptionResponse>
    {
        private readonly IPrescriptionRepository _prescriptionRepository;

        public UpdatePrescriptionHandler(IPrescriptionRepository prescriptionRepository)
        {
            _prescriptionRepository = prescriptionRepository;
        }

        public async Task<PrescriptionResponse> HandleAsync(UpdatePrescriptionCommand request, CancellationToken cancellationToken)
        {
            var prescription = await _prescriptionRepository.GetByIdAsync(request.PrescriptionId)
                ?? throw new KeyNotFoundException($"Prescription {request.PrescriptionId} not found.");

            prescription.IssueDate = request.IssueDate;
            prescription.Instruction = request.Instruction;

            _prescriptionRepository.Update(prescription);
            await _prescriptionRepository.SaveChangesAsync();

            return PrescriptionResponse.FromEntity(prescription);
        }
    }

    public class DeletePrescriptionRequest : IRequest<bool>
    {
        public required int PrescriptionId { get; init; }
    }

    public class DeletePrescriptionHandler : IRequestHandler<DeletePrescriptionRequest, bool>
    {
        private readonly IPrescriptionRepository _prescriptionRepository;
        private readonly IMedicineRepository _medicineRepository;

        public DeletePrescriptionHandler(IPrescriptionRepository prescriptionRepository, IMedicineRepository medicineRepository)
        {
            _prescriptionRepository = prescriptionRepository;
            _medicineRepository = medicineRepository;
        }

        public async Task<bool> HandleAsync(DeletePrescriptionRequest request, CancellationToken cancellationToken)
        {
            var prescription = await _prescriptionRepository.GetByIdAsync(request.PrescriptionId)
                ?? throw new KeyNotFoundException($"Prescription {request.PrescriptionId} not found.");

            // Deleting a prescription cascades to its items; restore the stock they consumed first.
            foreach (var item in prescription.PrescriptionItems)
            {
                var medicine = await _medicineRepository.GetByIdAsync(item.MedicineId);
                if (medicine is not null)
                {
                    medicine.StockQt += item.Quantity;
                    _medicineRepository.Update(medicine);
                }
            }

            _prescriptionRepository.Remove(prescription);
            await _prescriptionRepository.SaveChangesAsync();

            return true;
        }
    }
}
