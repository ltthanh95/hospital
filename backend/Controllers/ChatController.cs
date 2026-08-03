using System.Security.Claims;
using backend.Mediator.Interfaces;
using backend.Models;
using backend.Models.Dtos;
using backend.Services.Features;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IMyMediator _mediator;

        public ChatController(IMyMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("sessions")]
        [Authorize(Roles = nameof(Role.PATIENT))]
        public async Task<ActionResult<ApiResponse<ChatSessionResponse>>> GetOrCreateSession(CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _mediator.SendAsync(new GetOrCreateChatSessionRequest { RequestingUserId = userId }, cancellationToken);
            return Ok(ApiResponse<ChatSessionResponse>.Success(result));
        }

        [HttpGet("sessions/{id:int}/messages")]
        public async Task<ActionResult<ApiResponse<ChatSessionResponse>>> GetMessages(int id, CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _mediator.SendAsync(new GetChatMessagesRequest { ChatSessionId = id, RequestingUserId = userId }, cancellationToken);
            return Ok(ApiResponse<ChatSessionResponse>.Success(result));
        }

        [HttpGet("sessions")]
        [Authorize(Roles = nameof(Role.DOCTOR))]
        public async Task<ActionResult<ApiResponse<IEnumerable<ChatSessionResponse>>>> GetMySessions(CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _mediator.SendAsync(new GetMyDoctorChatSessionsRequest { RequestingUserId = userId }, cancellationToken);
            return Ok(ApiResponse<IEnumerable<ChatSessionResponse>>.Success(result));
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")
                ?? throw new UnauthorizedAccessException("User id claim is missing.");

            return int.Parse(userIdClaim);
        }
    }
}
