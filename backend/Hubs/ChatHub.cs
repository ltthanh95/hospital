using System.Security.Claims;
using backend.Models;
using backend.Models.Dtos;
using backend.Repositories.Interfaces;
using backend.Services.Chat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace backend.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatSessionRepository _chatSessionRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IChatBotService _chatBotService;
        private readonly IDoctorPresenceTracker _presenceTracker;

        public ChatHub(
            IChatSessionRepository chatSessionRepository,
            IDoctorRepository doctorRepository,
            IChatBotService chatBotService,
            IDoctorPresenceTracker presenceTracker)
        {
            _chatSessionRepository = chatSessionRepository;
            _doctorRepository = doctorRepository;
            _chatBotService = chatBotService;
            _presenceTracker = presenceTracker;
        }

        public override async Task OnConnectedAsync()
        {
            var (userId, role) = GetCurrentUser();
            if (role == Role.DOCTOR)
            {
                var doctor = await _doctorRepository.GetByUserIdAsync(userId);
                if (doctor is not null)
                {
                    _presenceTracker.MarkConnected(doctor.Id, Context.ConnectionId);
                }
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var (userId, role) = GetCurrentUser();
            if (role == Role.DOCTOR)
            {
                var doctor = await _doctorRepository.GetByUserIdAsync(userId);
                if (doctor is not null)
                {
                    _presenceTracker.MarkDisconnected(doctor.Id, Context.ConnectionId);
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinSession(int sessionId)
        {
            var (userId, _) = GetCurrentUser();
            var session = await GetSessionOrThrowAsync(sessionId);
            EnsureParticipant(session, userId);

            await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(sessionId));
        }

        public async Task SendMessage(int sessionId, string content)
        {
            var (userId, _) = GetCurrentUser();
            var session = await GetSessionOrThrowAsync(sessionId);
            var isPatient = session.Patient.UserId == userId;
            var isAssignedDoctor = session.Doctor?.UserId == userId;

            if (!isPatient && !isAssignedDoctor)
            {
                throw new HubException("You do not have access to this chat session.");
            }

            var senderRole = isPatient ? ChatSenderRole.PATIENT : ChatSenderRole.DOCTOR;
            var message = await _chatSessionRepository.AddMessageAsync(sessionId, senderRole, userId, content);
            await Clients.Group(GroupName(sessionId)).SendAsync("ReceiveMessage", ChatMessageResponse.FromEntity(message));

            if (session.Mode == ChatMode.BOT && isPatient)
            {
                var refreshed = await GetSessionOrThrowAsync(sessionId);
                var reply = await _chatBotService.GetReplyAsync(userId, refreshed.Messages, Context.ConnectionAborted);

                var botMessage = await _chatSessionRepository.AddMessageAsync(sessionId, ChatSenderRole.BOT, null, reply);
                await Clients.Group(GroupName(sessionId)).SendAsync("ReceiveMessage", ChatMessageResponse.FromEntity(botMessage));
            }
        }

        public async Task RequestDoctor(int sessionId)
        {
            var (userId, _) = GetCurrentUser();
            var session = await GetSessionOrThrowAsync(sessionId);

            if (session.Patient.UserId != userId)
            {
                throw new HubException("Only the patient can request a doctor for this session.");
            }

            if (_presenceTracker.TryGetAvailableDoctor(out var doctorId))
            {
                await AssignDoctorAsync(sessionId, doctorId);
            }
            else
            {
                session.Mode = ChatMode.WAITING_FOR_DOCTOR;
                _chatSessionRepository.Update(session);
                await _chatSessionRepository.SaveChangesAsync();

                _presenceTracker.Enqueue(sessionId);

                var systemMessage = await _chatSessionRepository.AddMessageAsync(
                    sessionId, ChatSenderRole.SYSTEM, null, "No doctors are available right now — you're in the queue and will be connected as soon as one is free.");
                await Clients.Group(GroupName(sessionId)).SendAsync("ReceiveMessage", ChatMessageResponse.FromEntity(systemMessage));
                await Clients.Group(GroupName(sessionId)).SendAsync("SessionModeChanged", sessionId, ChatMode.WAITING_FOR_DOCTOR.ToString(), null);
            }
        }

        public async Task SetAvailability(bool isAvailable)
        {
            var (userId, role) = GetCurrentUser();
            if (role != Role.DOCTOR)
            {
                throw new HubException("Only doctors can toggle availability.");
            }

            var doctor = await _doctorRepository.GetByUserIdAsync(userId)
                ?? throw new HubException("Doctor profile not found for the current user.");

            if (!isAvailable)
            {
                _presenceTracker.SetUnavailable(doctor.Id);
                return;
            }

            _presenceTracker.SetAvailable(doctor.Id);

            if (_presenceTracker.TryDequeueForDoctor(out var sessionId))
            {
                await AssignDoctorAsync(sessionId, doctor.Id);
            }
        }

        public async Task EndLiveChat(int sessionId)
        {
            var (userId, _) = GetCurrentUser();
            var session = await GetSessionOrThrowAsync(sessionId);
            EnsureParticipant(session, userId);

            session.Mode = ChatMode.BOT;
            session.DoctorId = null;
            _chatSessionRepository.Update(session);
            await _chatSessionRepository.SaveChangesAsync();

            var systemMessage = await _chatSessionRepository.AddMessageAsync(
                sessionId, ChatSenderRole.SYSTEM, null, "The live chat has ended. You're back with the assistant.");
            await Clients.Group(GroupName(sessionId)).SendAsync("ReceiveMessage", ChatMessageResponse.FromEntity(systemMessage));
            await Clients.Group(GroupName(sessionId)).SendAsync("SessionModeChanged", sessionId, ChatMode.BOT.ToString(), null);
        }

        private async Task AssignDoctorAsync(int sessionId, int doctorId)
        {
            var session = await GetSessionOrThrowAsync(sessionId);
            var doctor = await _doctorRepository.GetByIdAsync(doctorId);
            if (doctor is null)
            {
                return;
            }

            session.DoctorId = doctorId;
            session.Mode = ChatMode.LIVE;
            _chatSessionRepository.Update(session);
            await _chatSessionRepository.SaveChangesAsync();

            _presenceTracker.SetUnavailable(doctorId);

            foreach (var connectionId in _presenceTracker.GetConnections(doctorId))
            {
                await Groups.AddToGroupAsync(connectionId, GroupName(sessionId));
            }

            var doctorName = $"{doctor.FName} {doctor.LName}";
            var systemMessage = await _chatSessionRepository.AddMessageAsync(
                sessionId, ChatSenderRole.SYSTEM, null, $"You're now connected with Dr. {doctorName}.");
            await Clients.Group(GroupName(sessionId)).SendAsync("ReceiveMessage", ChatMessageResponse.FromEntity(systemMessage));
            await Clients.Group(GroupName(sessionId)).SendAsync("SessionModeChanged", sessionId, ChatMode.LIVE.ToString(), doctorName);
        }

        private async Task<ChatSession> GetSessionOrThrowAsync(int sessionId)
        {
            var session = await _chatSessionRepository.GetByIdAsync(sessionId);
            return session ?? throw new HubException($"Chat session {sessionId} not found.");
        }

        private static void EnsureParticipant(ChatSession session, int userId)
        {
            var isPatient = session.Patient.UserId == userId;
            var isAssignedDoctor = session.Doctor?.UserId == userId;

            if (!isPatient && !isAssignedDoctor)
            {
                throw new HubException("You do not have access to this chat session.");
            }
        }

        private (int UserId, Role Role) GetCurrentUser()
        {
            var userIdClaim = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? Context.User?.FindFirstValue("sub")
                ?? throw new HubException("User id claim is missing.");
            var roleClaim = Context.User?.FindFirstValue(ClaimTypes.Role)
                ?? throw new HubException("Role claim is missing.");

            return (int.Parse(userIdClaim), Enum.Parse<Role>(roleClaim));
        }

        private static string GroupName(int sessionId) => $"chat-session-{sessionId}";
    }
}
