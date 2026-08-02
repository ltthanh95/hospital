using backend.Mediator.Interfaces;
using backend.Models;
using backend.Models.Dtos;
using backend.Repositories.Interfaces;

namespace backend.Services.Features
{
    public class GetAllPrescriptionItemRequest : IRequest<IEnumerable<PrescriptionItemResponse>>
    {
    }

    public class GetAllPrescriptionItemHandler : IRequestHandler<GetAllPrescriptionItemRequest, IEnumerable<PrescriptionItemResponse>>
    {
        private readonly IPrescriptionItemRepository _prescriptionItemRepository;

        public GetAllPrescriptionItemHandler(IPrescriptionItemRepository prescriptionItemRepository)
        {
            _prescriptionItemRepository = prescriptionItemRepository;
        }

        public async Task<IEnumerable<PrescriptionItemResponse>> HandleAsync(GetAllPrescriptionItemRequest request, CancellationToken cancellationToken)
        {
            var items = await _prescriptionItemRepository.GetAllAsync();
            return items.Select(PrescriptionItemResponse.FromEntity);
        }
    }

    public class GetPrescriptionItemByIdRequest : IRequest<PrescriptionItemResponse>
    {
        public required int PrescriptionItemId { get; init; }
    }

    public class GetPrescriptionItemByIdHandler : IRequestHandler<GetPrescriptionItemByIdRequest, PrescriptionItemResponse>
    {
        private readonly IPrescriptionItemRepository _prescriptionItemRepository;

        public GetPrescriptionItemByIdHandler(IPrescriptionItemRepository prescriptionItemRepository)
        {
            _prescriptionItemRepository = prescriptionItemRepository;
        }

        public async Task<PrescriptionItemResponse> HandleAsync(GetPrescriptionItemByIdRequest request, CancellationToken cancellationToken)
        {
            var item = await _prescriptionItemRepository.GetByIdAsync(request.PrescriptionItemId)
                ?? throw new KeyNotFoundException($"PrescriptionItem {request.PrescriptionItemId} not found.");

            return PrescriptionItemResponse.FromEntity(item);
        }
    }

    public class CreatePrescriptionItemCommand : IRequest<PrescriptionItemResponse>
    {
        public required int PrescriptionId { get; init; }
        public required int MedicineId { get; init; }
        public required string Dosage { get; init; }
        public required int Quantity { get; init; }
        public required string Frequency { get; init; }
        public required int DurationDays { get; init; }
    }

    public class CreatePrescriptionItemHandler : IRequestHandler<CreatePrescriptionItemCommand, PrescriptionItemResponse>
    {
        private readonly IPrescriptionItemRepository _prescriptionItemRepository;
        private readonly IPrescriptionRepository _prescriptionRepository;
        private readonly IMedicineRepository _medicineRepository;

        public CreatePrescriptionItemHandler(
            IPrescriptionItemRepository prescriptionItemRepository,
            IPrescriptionRepository prescriptionRepository,
            IMedicineRepository medicineRepository)
        {
            _prescriptionItemRepository = prescriptionItemRepository;
            _prescriptionRepository = prescriptionRepository;
            _medicineRepository = medicineRepository;
        }

        public async Task<PrescriptionItemResponse> HandleAsync(CreatePrescriptionItemCommand request, CancellationToken cancellationToken)
        {
            _ = await _prescriptionRepository.GetByIdAsync(request.PrescriptionId)
                ?? throw new KeyNotFoundException($"Prescription {request.PrescriptionId} not found.");

            var medicine = await _medicineRepository.GetByIdAsync(request.MedicineId)
                ?? throw new KeyNotFoundException($"Medicine {request.MedicineId} not found. Please choose a different medicine.");

            if (medicine.StockQt < request.Quantity)
            {
                throw new InvalidOperationException(
                    $"Medicine '{medicine.Name}' is out of stock (available: {medicine.StockQt}, requested: {request.Quantity}). Please choose a different medicine.");
            }

            medicine.StockQt -= request.Quantity;
            _medicineRepository.Update(medicine);

            var item = new PrescriptionItem
            {
                PrescriptionId = request.PrescriptionId,
                MedicineId = request.MedicineId,
                Dosage = request.Dosage,
                Quantity = request.Quantity,
                Frequency = request.Frequency,
                durationDays = request.DurationDays,
            };

            await _prescriptionItemRepository.AddAsync(item);
            await _prescriptionItemRepository.SaveChangesAsync();

            var created = await _prescriptionItemRepository.GetByIdAsync(item.Id)
                ?? throw new KeyNotFoundException("PrescriptionItem not found after creation.");

            return PrescriptionItemResponse.FromEntity(created);
        }
    }

    public class UpdatePrescriptionItemCommand : IRequest<PrescriptionItemResponse>
    {
        public required int PrescriptionItemId { get; init; }
        public required int MedicineId { get; init; }
        public required string Dosage { get; init; }
        public required int Quantity { get; init; }
        public required string Frequency { get; init; }
        public required int DurationDays { get; init; }
    }

    public class UpdatePrescriptionItemHandler : IRequestHandler<UpdatePrescriptionItemCommand, PrescriptionItemResponse>
    {
        private readonly IPrescriptionItemRepository _prescriptionItemRepository;
        private readonly IMedicineRepository _medicineRepository;

        public UpdatePrescriptionItemHandler(
            IPrescriptionItemRepository prescriptionItemRepository,
            IMedicineRepository medicineRepository)
        {
            _prescriptionItemRepository = prescriptionItemRepository;
            _medicineRepository = medicineRepository;
        }

        public async Task<PrescriptionItemResponse> HandleAsync(UpdatePrescriptionItemCommand request, CancellationToken cancellationToken)
        {
            var item = await _prescriptionItemRepository.GetByIdAsync(request.PrescriptionItemId)
                ?? throw new KeyNotFoundException($"PrescriptionItem {request.PrescriptionItemId} not found.");

            // Restore the stock the existing item consumed, then re-validate and consume against the new values.
            var previousMedicine = await _medicineRepository.GetByIdAsync(item.MedicineId);
            if (previousMedicine is not null)
            {
                previousMedicine.StockQt += item.Quantity;
                _medicineRepository.Update(previousMedicine);
            }

            var newMedicine = await _medicineRepository.GetByIdAsync(request.MedicineId)
                ?? throw new KeyNotFoundException($"Medicine {request.MedicineId} not found. Please choose a different medicine.");

            if (newMedicine.StockQt < request.Quantity)
            {
                throw new InvalidOperationException(
                    $"Medicine '{newMedicine.Name}' is out of stock (available: {newMedicine.StockQt}, requested: {request.Quantity}). Please choose a different medicine.");
            }

            newMedicine.StockQt -= request.Quantity;
            _medicineRepository.Update(newMedicine);

            item.MedicineId = request.MedicineId;
            item.Dosage = request.Dosage;
            item.Quantity = request.Quantity;
            item.Frequency = request.Frequency;
            item.durationDays = request.DurationDays;

            _prescriptionItemRepository.Update(item);
            await _prescriptionItemRepository.SaveChangesAsync();

            var updated = await _prescriptionItemRepository.GetByIdAsync(item.Id)
                ?? throw new KeyNotFoundException("PrescriptionItem not found after update.");

            return PrescriptionItemResponse.FromEntity(updated);
        }
    }

    public class DeletePrescriptionItemRequest : IRequest<bool>
    {
        public required int PrescriptionItemId { get; init; }
    }

    public class DeletePrescriptionItemHandler : IRequestHandler<DeletePrescriptionItemRequest, bool>
    {
        private readonly IPrescriptionItemRepository _prescriptionItemRepository;
        private readonly IMedicineRepository _medicineRepository;

        public DeletePrescriptionItemHandler(
            IPrescriptionItemRepository prescriptionItemRepository,
            IMedicineRepository medicineRepository)
        {
            _prescriptionItemRepository = prescriptionItemRepository;
            _medicineRepository = medicineRepository;
        }

        public async Task<bool> HandleAsync(DeletePrescriptionItemRequest request, CancellationToken cancellationToken)
        {
            var item = await _prescriptionItemRepository.GetByIdAsync(request.PrescriptionItemId)
                ?? throw new KeyNotFoundException($"PrescriptionItem {request.PrescriptionItemId} not found.");

            var medicine = await _medicineRepository.GetByIdAsync(item.MedicineId);
            if (medicine is not null)
            {
                medicine.StockQt += item.Quantity;
                _medicineRepository.Update(medicine);
            }

            _prescriptionItemRepository.Remove(item);
            await _prescriptionItemRepository.SaveChangesAsync();

            return true;
        }
    }
}
