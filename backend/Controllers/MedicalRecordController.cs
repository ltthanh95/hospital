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
    [Authorize(Roles = nameof(Role.DOCTOR))]
    public class MedicalRecordController : ControllerBase
    {
        private readonly IMyMediator _mediator;

        public MedicalRecordController(IMyMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<MedicalRecordResponse>>>> GetAll(CancellationToken cancellationToken)
        {
            var result = await _mediator.SendAsync(new GetAllMedicalRecordRequest(), cancellationToken);
            return Ok(ApiResponse<IEnumerable<MedicalRecordResponse>>.Success(result));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<MedicalRecordResponse>>> GetById(int id, CancellationToken cancellationToken)
        {
            var result = await _mediator.SendAsync(new GetMedicalRecordByIdRequest { MedicalRecordId = id }, cancellationToken);
            return Ok(ApiResponse<MedicalRecordResponse>.Success(result));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<MedicalRecordResponse>>> Create(CreateMedicalRecordRequest request, CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _mediator.SendAsync(new CreateMedicalRecordCommand
            {
                PatientId = request.PatientId,
                AppointmentId = request.AppointmentId,
                Visit = request.Visit,
                Diagnosis = request.Diagnosis,
                Notes = request.Notes,
                RequestingUserId = userId,
            }, cancellationToken);

            var response = ApiResponse<MedicalRecordResponse>.Success(result, "Medical record created.", StatusCodes.Status201Created);
            return StatusCode(StatusCodes.Status201Created, response);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponse<MedicalRecordResponse>>> Update(int id, UpdateMedicalRecordRequest request, CancellationToken cancellationToken)
        {
            var result = await _mediator.SendAsync(new UpdateMedicalRecordCommand
            {
                MedicalRecordId = id,
                Visit = request.Visit,
                Diagnosis = request.Diagnosis,
                Notes = request.Notes,
            }, cancellationToken);

            return Ok(ApiResponse<MedicalRecordResponse>.Success(result, "Medical record updated."));
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponse>> Delete(int id, CancellationToken cancellationToken)
        {
            await _mediator.SendAsync(new DeleteMedicalRecordRequest { MedicalRecordId = id }, cancellationToken);
            return Ok(ApiResponse.Success("Medical record deleted."));
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
