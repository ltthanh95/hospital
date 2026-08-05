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
    public class PatientController : ControllerBase
    {
        private readonly IMyMediator _mediator;

        public PatientController(IMyMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<PatientResponse>>>> GetAll(CancellationToken cancellationToken)
        {
            var result = await _mediator.SendAsync(new GetAllPatientRequest(), cancellationToken);
            return Ok(ApiResponse<IEnumerable<PatientResponse>>.Success(result));
        }

        [HttpGet("me")]
        [Authorize(Roles = nameof(Role.PATIENT))]
        public async Task<ActionResult<ApiResponse<PatientResponse>>> GetMe(CancellationToken cancellationToken)
        {
            var (userId, _) = GetCurrentUser();
            var result = await _mediator.SendAsync(new GetMyPatientRequest { RequestingUserId = userId }, cancellationToken);
            return Ok(ApiResponse<PatientResponse>.Success(result));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<PatientResponse>>> GetById(int id, CancellationToken cancellationToken)
        {
            var result = await _mediator.SendAsync(new GetPatientByIdRequest { PatientId = id }, cancellationToken);
            return Ok(ApiResponse<PatientResponse>.Success(result));
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponse<PatientResponse>>> Update(int id, PatientUpdateDetails request, CancellationToken cancellationToken)
        {
            var (userId, role) = GetCurrentUser();
            var result = await _mediator.SendAsync(new UpdatePatientRequest
            {
                PatientId = id,
                Details = request,
                RequestingUserId = userId,
                RequestingUserRole = role,
            }, cancellationToken);

            return Ok(ApiResponse<PatientResponse>.Success(result, "Patient updated."));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = nameof(Role.ADMIN))]
        public async Task<ActionResult<ApiResponse>> Delete(int id, CancellationToken cancellationToken)
        {
            await _mediator.SendAsync(new DeletePatientRequest { PatientId = id }, cancellationToken);
            return Ok(ApiResponse.Success("Patient deleted."));
        }

        [HttpPatch("{id:int}/status")]
        [Authorize(Roles = $"{nameof(Role.DOCTOR)},{nameof(Role.ADMIN)}")]
        public async Task<ActionResult<ApiResponse<PatientResponse>>> UpdateStatus(int id, UpdatePatientStatusBody request, CancellationToken cancellationToken)
        {
            var result = await _mediator.SendAsync(new UpdatePatientStatusRequest
            {
                PatientId = id,
                Status = request.Status,
            }, cancellationToken);

            return Ok(ApiResponse<PatientResponse>.Success(result, "Patient status updated."));
        }

        private (int UserId, Role Role) GetCurrentUser()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")
                ?? throw new UnauthorizedAccessException("User id claim is missing.");
            var roleClaim = User.FindFirstValue(ClaimTypes.Role)
                ?? throw new UnauthorizedAccessException("Role claim is missing.");

            return (int.Parse(userIdClaim), Enum.Parse<Role>(roleClaim));
        }
    }
}
