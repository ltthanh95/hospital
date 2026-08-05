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
    public class DoctorController : ControllerBase
    {
        private readonly IMyMediator _mediator;

        public DoctorController(IMyMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<DoctorResponse>>>> GetAll(CancellationToken cancellationToken)
        {
            var result = await _mediator.SendAsync(new GetAllDoctorRequest(), cancellationToken);
            return Ok(ApiResponse<IEnumerable<DoctorResponse>>.Success(result));
        }

        [HttpGet("me")]
        [Authorize(Roles = nameof(Role.DOCTOR))]
        public async Task<ActionResult<ApiResponse<DoctorResponse>>> GetMe(CancellationToken cancellationToken)
        {
            var (userId, _) = GetCurrentUser();
            var result = await _mediator.SendAsync(new GetMyDoctorRequest { RequestingUserId = userId }, cancellationToken);
            return Ok(ApiResponse<DoctorResponse>.Success(result));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<DoctorResponse>>> GetById(int id, CancellationToken cancellationToken)
        {
            var result = await _mediator.SendAsync(new GetDoctorByIdRequest { DoctorId = id }, cancellationToken);
            return Ok(ApiResponse<DoctorResponse>.Success(result));
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponse<DoctorResponse>>> Update(int id, DoctorUpdateDetails request, CancellationToken cancellationToken)
        {
            var (userId, role) = GetCurrentUser();
            var result = await _mediator.SendAsync(new UpdateDoctorRequest
            {
                DoctorId = id,
                Details = request,
                RequestingUserId = userId,
                RequestingUserRole = role,
            }, cancellationToken);

            return Ok(ApiResponse<DoctorResponse>.Success(result, "Doctor updated."));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = nameof(Role.ADMIN))]
        public async Task<ActionResult<ApiResponse>> Delete(int id, CancellationToken cancellationToken)
        {
            await _mediator.SendAsync(new DeleteDoctorRequest { DoctorId = id }, cancellationToken);
            return Ok(ApiResponse.Success("Doctor deleted."));
        }

        [HttpPatch("{id:int}/salary")]
        [Authorize(Roles = nameof(Role.ADMIN))]
        public async Task<ActionResult<ApiResponse<DoctorResponse>>> UpdateSalary(int id, UpdateDoctorSalaryBody request, CancellationToken cancellationToken)
        {
            var result = await _mediator.SendAsync(new UpdateDoctorSalaryRequest
            {
                DoctorId = id,
                Salary = request.Salary,
            }, cancellationToken);

            return Ok(ApiResponse<DoctorResponse>.Success(result, "Doctor salary updated."));
        }

        [HttpPatch("{id:int}/status")]
        [Authorize(Roles = nameof(Role.ADMIN))]
        public async Task<ActionResult<ApiResponse<DoctorResponse>>> UpdateStatus(int id, UpdateDoctorStatusBody request, CancellationToken cancellationToken)
        {
            var result = await _mediator.SendAsync(new UpdateDoctorStatusRequest
            {
                DoctorId = id,
                Status = request.Status,
            }, cancellationToken);

            return Ok(ApiResponse<DoctorResponse>.Success(result, "Doctor status updated."));
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
