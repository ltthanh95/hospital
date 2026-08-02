using backend.Mediator.Interfaces;
using backend.Models;
using backend.Models.Dtos;
using backend.Repositories.Interfaces;

namespace backend.Services.Features
{
    public class GetAllMedicineRequest : IRequest<IEnumerable<MedicineResponse>>
    {
    }

    public class GetAllMedicineHandler : IRequestHandler<GetAllMedicineRequest, IEnumerable<MedicineResponse>>
    {
        private readonly IMedicineRepository _medicineRepository;

        public GetAllMedicineHandler(IMedicineRepository medicineRepository)
        {
            _medicineRepository = medicineRepository;
        }

        public async Task<IEnumerable<MedicineResponse>> HandleAsync(GetAllMedicineRequest request, CancellationToken cancellationToken)
        {
            var medicines = await _medicineRepository.GetAllAsync();
            return medicines.Select(MedicineResponse.FromEntity);
        }
    }

    public class GetMedicineByIdRequest : IRequest<MedicineResponse>
    {
        public required int MedicineId { get; init; }
    }

    public class GetMedicineByIdHandler : IRequestHandler<GetMedicineByIdRequest, MedicineResponse>
    {
        private readonly IMedicineRepository _medicineRepository;

        public GetMedicineByIdHandler(IMedicineRepository medicineRepository)
        {
            _medicineRepository = medicineRepository;
        }

        public async Task<MedicineResponse> HandleAsync(GetMedicineByIdRequest request, CancellationToken cancellationToken)
        {
            var medicine = await _medicineRepository.GetByIdAsync(request.MedicineId)
                ?? throw new KeyNotFoundException($"Medicine {request.MedicineId} not found.");

            return MedicineResponse.FromEntity(medicine);
        }
    }

    public class CreateMedicineCommand : IRequest<MedicineResponse>
    {
        public required string Name { get; init; }
        public required string Manufacturer { get; init; }
        public required decimal UnitPrice { get; init; }
        public required int StockQt { get; init; }
        public required DateTime Expiration { get; init; }
    }

    public class CreateMedicineHandler : IRequestHandler<CreateMedicineCommand, MedicineResponse>
    {
        private readonly IMedicineRepository _medicineRepository;

        public CreateMedicineHandler(IMedicineRepository medicineRepository)
        {
            _medicineRepository = medicineRepository;
        }

        public async Task<MedicineResponse> HandleAsync(CreateMedicineCommand request, CancellationToken cancellationToken)
        {
            var medicine = new Medicine
            {
                Name = request.Name,
                Manufacturer = request.Manufacturer,
                UnitPrice = request.UnitPrice,
                StockQt = request.StockQt,
                Expiration = request.Expiration,
            };

            await _medicineRepository.AddAsync(medicine);
            await _medicineRepository.SaveChangesAsync();

            return MedicineResponse.FromEntity(medicine);
        }
    }

    public class UpdateMedicineCommand : IRequest<MedicineResponse>
    {
        public required int MedicineId { get; init; }
        public required string Name { get; init; }
        public required string Manufacturer { get; init; }
        public required decimal UnitPrice { get; init; }
        public required int StockQt { get; init; }
        public required DateTime Expiration { get; init; }
    }

    public class UpdateMedicineHandler : IRequestHandler<UpdateMedicineCommand, MedicineResponse>
    {
        private readonly IMedicineRepository _medicineRepository;

        public UpdateMedicineHandler(IMedicineRepository medicineRepository)
        {
            _medicineRepository = medicineRepository;
        }

        public async Task<MedicineResponse> HandleAsync(UpdateMedicineCommand request, CancellationToken cancellationToken)
        {
            var medicine = await _medicineRepository.GetByIdAsync(request.MedicineId)
                ?? throw new KeyNotFoundException($"Medicine {request.MedicineId} not found.");

            medicine.Name = request.Name;
            medicine.Manufacturer = request.Manufacturer;
            medicine.UnitPrice = request.UnitPrice;
            medicine.StockQt = request.StockQt;
            medicine.Expiration = request.Expiration;

            _medicineRepository.Update(medicine);
            await _medicineRepository.SaveChangesAsync();

            return MedicineResponse.FromEntity(medicine);
        }
    }

    public class UpdateMedicineStockCommand : IRequest<MedicineResponse>
    {
        public required int MedicineId { get; init; }
        public required int StockQt { get; init; }
    }

    public class UpdateMedicineStockHandler : IRequestHandler<UpdateMedicineStockCommand, MedicineResponse>
    {
        private readonly IMedicineRepository _medicineRepository;

        public UpdateMedicineStockHandler(IMedicineRepository medicineRepository)
        {
            _medicineRepository = medicineRepository;
        }

        public async Task<MedicineResponse> HandleAsync(UpdateMedicineStockCommand request, CancellationToken cancellationToken)
        {
            var medicine = await _medicineRepository.GetByIdAsync(request.MedicineId)
                ?? throw new KeyNotFoundException($"Medicine {request.MedicineId} not found.");

            medicine.StockQt = request.StockQt;

            _medicineRepository.Update(medicine);
            await _medicineRepository.SaveChangesAsync();

            return MedicineResponse.FromEntity(medicine);
        }
    }

    public class DeleteMedicineRequest : IRequest<bool>
    {
        public required int MedicineId { get; init; }
    }

    public class DeleteMedicineHandler : IRequestHandler<DeleteMedicineRequest, bool>
    {
        private readonly IMedicineRepository _medicineRepository;

        public DeleteMedicineHandler(IMedicineRepository medicineRepository)
        {
            _medicineRepository = medicineRepository;
        }

        public async Task<bool> HandleAsync(DeleteMedicineRequest request, CancellationToken cancellationToken)
        {
            var medicine = await _medicineRepository.GetByIdAsync(request.MedicineId)
                ?? throw new KeyNotFoundException($"Medicine {request.MedicineId} not found.");

            _medicineRepository.Remove(medicine);
            await _medicineRepository.SaveChangesAsync();

            return true;
        }
    }
}
