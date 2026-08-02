using backend.Mediator.Interfaces;
using backend.Models;
using backend.Models.Dtos;
using backend.Repositories.Interfaces;

namespace backend.Services.Features
{
    public class GetAllRoomRequest : IRequest<IEnumerable<RoomResponse>>
    {
    }

    public class GetAllRoomHandler : IRequestHandler<GetAllRoomRequest, IEnumerable<RoomResponse>>
    {
        private readonly IRoomRepository _roomRepository;

        public GetAllRoomHandler(IRoomRepository roomRepository)
        {
            _roomRepository = roomRepository;
        }

        public async Task<IEnumerable<RoomResponse>> HandleAsync(GetAllRoomRequest request, CancellationToken cancellationToken)
        {
            var rooms = await _roomRepository.GetAllAsync();
            return rooms.Select(RoomResponse.FromEntity);
        }
    }

    public class GetRoomByIdRequest : IRequest<RoomResponse>
    {
        public required int RoomId { get; init; }
    }

    public class GetRoomByIdHandler : IRequestHandler<GetRoomByIdRequest, RoomResponse>
    {
        private readonly IRoomRepository _roomRepository;

        public GetRoomByIdHandler(IRoomRepository roomRepository)
        {
            _roomRepository = roomRepository;
        }

        public async Task<RoomResponse> HandleAsync(GetRoomByIdRequest request, CancellationToken cancellationToken)
        {
            var room = await _roomRepository.GetByIdAsync(request.RoomId)
                ?? throw new KeyNotFoundException($"Room {request.RoomId} not found.");

            return RoomResponse.FromEntity(room);
        }
    }

    public class CreateRoomCommand : IRequest<RoomResponse>
    {
        public required string RoomNumber { get; init; }
        public required string Type { get; init; }
        public required int Capacity { get; init; }
    }

    public class CreateRoomHandler : IRequestHandler<CreateRoomCommand, RoomResponse>
    {
        private readonly IRoomRepository _roomRepository;

        public CreateRoomHandler(IRoomRepository roomRepository)
        {
            _roomRepository = roomRepository;
        }

        public async Task<RoomResponse> HandleAsync(CreateRoomCommand request, CancellationToken cancellationToken)
        {
            var room = new Room
            {
                RoomNumber = request.RoomNumber,
                Type = request.Type,
                Capacity = request.Capacity,
            };

            await _roomRepository.AddAsync(room);
            await _roomRepository.SaveChangesAsync();

            return RoomResponse.FromEntity(room);
        }
    }

    public class UpdateRoomCommand : IRequest<RoomResponse>
    {
        public required int RoomId { get; init; }
        public required string RoomNumber { get; init; }
        public required string Type { get; init; }
        public required int Capacity { get; init; }
    }

    public class UpdateRoomHandler : IRequestHandler<UpdateRoomCommand, RoomResponse>
    {
        private readonly IRoomRepository _roomRepository;

        public UpdateRoomHandler(IRoomRepository roomRepository)
        {
            _roomRepository = roomRepository;
        }

        public async Task<RoomResponse> HandleAsync(UpdateRoomCommand request, CancellationToken cancellationToken)
        {
            var room = await _roomRepository.GetByIdAsync(request.RoomId)
                ?? throw new KeyNotFoundException($"Room {request.RoomId} not found.");

            room.RoomNumber = request.RoomNumber;
            room.Type = request.Type;
            room.Capacity = request.Capacity;

            _roomRepository.Update(room);
            await _roomRepository.SaveChangesAsync();

            return RoomResponse.FromEntity(room);
        }
    }

    public class DeleteRoomRequest : IRequest<bool>
    {
        public required int RoomId { get; init; }
    }

    public class DeleteRoomHandler : IRequestHandler<DeleteRoomRequest, bool>
    {
        private readonly IRoomRepository _roomRepository;

        public DeleteRoomHandler(IRoomRepository roomRepository)
        {
            _roomRepository = roomRepository;
        }

        public async Task<bool> HandleAsync(DeleteRoomRequest request, CancellationToken cancellationToken)
        {
            var room = await _roomRepository.GetByIdAsync(request.RoomId)
                ?? throw new KeyNotFoundException($"Room {request.RoomId} not found.");

            _roomRepository.Remove(room);
            await _roomRepository.SaveChangesAsync();

            return true;
        }
    }
}
