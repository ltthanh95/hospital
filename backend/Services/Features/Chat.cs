using backend.Exceptions;
using backend.Mediator.Interfaces;
using backend.Models;
using backend.Models.Dtos;
using backend.Repositories.Interfaces;

namespace backend.Services.Features
{
    public class GetOrCreateChatSessionRequest : IRequest<ChatSessionResponse>
    {
        public required int RequestingUserId { get; init; }
    }

    public class GetOrCreateChatSessionHandler : IRequestHandler<GetOrCreateChatSessionRequest, ChatSessionResponse>
    {
        private readonly IChatSessionRepository _chatSessionRepository;
        private readonly IPatientRepository _patientRepository;

        public GetOrCreateChatSessionHandler(IChatSessionRepository chatSessionRepository, IPatientRepository patientRepository)
        {
            _chatSessionRepository = chatSessionRepository;
            _patientRepository = patientRepository;
        }

        public async Task<ChatSessionResponse> HandleAsync(GetOrCreateChatSessionRequest request, CancellationToken cancellationToken)
        {
            var patient = await _patientRepository.GetByUserIdAsync(request.RequestingUserId)
                ?? throw new KeyNotFoundException("Patient profile not found for the current user.");

            var existing = await _chatSessionRepository.GetMostRecentOpenSessionAsync(patient.Id);
            if (existing is not null)
            {
                return ChatSessionResponse.FromEntity(existing);
            }

            var session = new ChatSession
            {
                PatientId = patient.Id,
                Mode = ChatMode.BOT,
            };

            await _chatSessionRepository.AddAsync(session);
            await _chatSessionRepository.SaveChangesAsync();

            var created = await _chatSessionRepository.GetByIdAsync(session.Id)
                ?? throw new KeyNotFoundException("Chat session not found after creation.");

            return ChatSessionResponse.FromEntity(created);
        }
    }

    public class GetChatMessagesRequest : IRequest<ChatSessionResponse>
    {
        public required int ChatSessionId { get; init; }
        public required int RequestingUserId { get; init; }
    }

    public class GetChatMessagesHandler : IRequestHandler<GetChatMessagesRequest, ChatSessionResponse>
    {
        private readonly IChatSessionRepository _chatSessionRepository;

        public GetChatMessagesHandler(IChatSessionRepository chatSessionRepository)
        {
            _chatSessionRepository = chatSessionRepository;
        }

        public async Task<ChatSessionResponse> HandleAsync(GetChatMessagesRequest request, CancellationToken cancellationToken)
        {
            var session = await _chatSessionRepository.GetByIdAsync(request.ChatSessionId)
                ?? throw new KeyNotFoundException($"Chat session {request.ChatSessionId} not found.");

            var isPatient = session.Patient.UserId == request.RequestingUserId;
            var isAssignedDoctor = session.Doctor?.UserId == request.RequestingUserId;

            if (!isPatient && !isAssignedDoctor)
            {
                throw new ForbiddenAccessException("You do not have access to this chat session.");
            }

            return ChatSessionResponse.FromEntity(session);
        }
    }

    public class GetMyDoctorChatSessionsRequest : IRequest<IEnumerable<ChatSessionResponse>>
    {
        public required int RequestingUserId { get; init; }
    }

    public class GetMyDoctorChatSessionsHandler : IRequestHandler<GetMyDoctorChatSessionsRequest, IEnumerable<ChatSessionResponse>>
    {
        private readonly IChatSessionRepository _chatSessionRepository;
        private readonly IDoctorRepository _doctorRepository;

        public GetMyDoctorChatSessionsHandler(IChatSessionRepository chatSessionRepository, IDoctorRepository doctorRepository)
        {
            _chatSessionRepository = chatSessionRepository;
            _doctorRepository = doctorRepository;
        }

        public async Task<IEnumerable<ChatSessionResponse>> HandleAsync(GetMyDoctorChatSessionsRequest request, CancellationToken cancellationToken)
        {
            var doctor = await _doctorRepository.GetByUserIdAsync(request.RequestingUserId)
                ?? throw new KeyNotFoundException("Doctor profile not found for the current user.");

            var sessions = await _chatSessionRepository.GetForDoctorAsync(doctor.Id);
            return sessions.Select(ChatSessionResponse.FromEntity);
        }
    }
}
