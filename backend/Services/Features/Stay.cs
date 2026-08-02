using backend.Mediator.Interfaces;
using backend.Models;
using backend.Models.Dtos;
using backend.Repositories.Interfaces;

namespace backend.Services.Features
{
    public class GetAllStayRequest : IRequest<IEnumerable<StayResponse>>
    {
    }

    public class GetAllStayHandler : IRequestHandler<GetAllStayRequest, IEnumerable<StayResponse>>
    {
        private readonly IStayRepository _stayRepository;

        public GetAllStayHandler(IStayRepository stayRepository)
        {
            _stayRepository = stayRepository;
        }

        public async Task<IEnumerable<StayResponse>> HandleAsync(GetAllStayRequest request, CancellationToken cancellationToken)
        {
            var stays = await _stayRepository.GetAllAsync();
            return stays.Select(StayResponse.FromEntity);
        }
    }

    public class GetStayByIdRequest : IRequest<StayResponse>
    {
        public required int StayId { get; init; }
    }

    public class GetStayByIdHandler : IRequestHandler<GetStayByIdRequest, StayResponse>
    {
        private readonly IStayRepository _stayRepository;

        public GetStayByIdHandler(IStayRepository stayRepository)
        {
            _stayRepository = stayRepository;
        }

        public async Task<StayResponse> HandleAsync(GetStayByIdRequest request, CancellationToken cancellationToken)
        {
            var stay = await _stayRepository.GetByIdAsync(request.StayId)
                ?? throw new KeyNotFoundException($"Stay {request.StayId} not found.");

            return StayResponse.FromEntity(stay);
        }
    }

    public class CreateStayCommand : IRequest<StayResponse>
    {
        public required int PatientId { get; init; }
        public required int RoomId { get; init; }
        public required DateTime CheckIn { get; init; }
    }

    public class CreateStayHandler : IRequestHandler<CreateStayCommand, StayResponse>
    {
        private readonly IStayRepository _stayRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IRoomRepository _roomRepository;

        public CreateStayHandler(
            IStayRepository stayRepository,
            IPatientRepository patientRepository,
            IRoomRepository roomRepository)
        {
            _stayRepository = stayRepository;
            _patientRepository = patientRepository;
            _roomRepository = roomRepository;
        }

        public async Task<StayResponse> HandleAsync(CreateStayCommand request, CancellationToken cancellationToken)
        {
            _ = await _patientRepository.GetByIdAsync(request.PatientId)
                ?? throw new KeyNotFoundException($"Patient {request.PatientId} not found.");

            var room = await _roomRepository.GetByIdAsync(request.RoomId)
                ?? throw new KeyNotFoundException($"Room {request.RoomId} not found.");

            var occupancy = await _stayRepository.GetActiveOccupancyAsync(request.RoomId);
            if (occupancy >= room.Capacity)
            {
                throw new InvalidOperationException($"Room {room.RoomNumber} is at full capacity ({occupancy}/{room.Capacity}).");
            }

            var stay = new Stay
            {
                PatientId = request.PatientId,
                RoomId = request.RoomId,
                CheckIn = request.CheckIn,
            };

            await _stayRepository.AddAsync(stay);
            await _stayRepository.SaveChangesAsync();

            var created = await _stayRepository.GetByIdAsync(stay.Id)
                ?? throw new KeyNotFoundException("Stay not found after creation.");

            return StayResponse.FromEntity(created);
        }
    }

    public class CheckoutStayRequest : IRequest<StayResponse>
    {
        public required int StayId { get; init; }
    }

    public class CheckoutStayHandler : IRequestHandler<CheckoutStayRequest, StayResponse>
    {
        private readonly IStayRepository _stayRepository;

        public CheckoutStayHandler(IStayRepository stayRepository)
        {
            _stayRepository = stayRepository;
        }

        public async Task<StayResponse> HandleAsync(CheckoutStayRequest request, CancellationToken cancellationToken)
        {
            var stay = await _stayRepository.GetByIdAsync(request.StayId)
                ?? throw new KeyNotFoundException($"Stay {request.StayId} not found.");

            if (stay.CheckOut is not null)
            {
                throw new InvalidOperationException("This stay has already been checked out.");
            }

            stay.CheckOut = DateTime.UtcNow;

            _stayRepository.Update(stay);
            await _stayRepository.SaveChangesAsync();

            return StayResponse.FromEntity(stay);
        }
    }

    public class DeleteStayRequest : IRequest<bool>
    {
        public required int StayId { get; init; }
    }

    public class DeleteStayHandler : IRequestHandler<DeleteStayRequest, bool>
    {
        private readonly IStayRepository _stayRepository;

        public DeleteStayHandler(IStayRepository stayRepository)
        {
            _stayRepository = stayRepository;
        }

        public async Task<bool> HandleAsync(DeleteStayRequest request, CancellationToken cancellationToken)
        {
            var stay = await _stayRepository.GetByIdAsync(request.StayId)
                ?? throw new KeyNotFoundException($"Stay {request.StayId} not found.");

            _stayRepository.Remove(stay);
            await _stayRepository.SaveChangesAsync();

            return true;
        }
    }
}
