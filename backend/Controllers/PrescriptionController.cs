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
    public class PrescriptionController : ControllerBase
    {
        private readonly IMyMediator _mediator;

        public PrescriptionController(IMyMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<PrescriptionResponse>>>> GetAll(CancellationToken cancellationToken)
        {
            var result = await _mediator.SendAsync(new GetAllPrescriptionRequest(), cancellationToken);
            return Ok(ApiResponse<IEnumerable<PrescriptionResponse>>.Success(result));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<PrescriptionResponse>>> GetById(int id, CancellationToken cancellationToken)
        {
            var result = await _mediator.SendAsync(new GetPrescriptionByIdRequest { PrescriptionId = id }, cancellationToken);
            return Ok(ApiResponse<PrescriptionResponse>.Success(result));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<PrescriptionResponse>>> Create(CreatePrescriptionRequest request, CancellationToken cancellationToken)
        {
            var result = await _mediator.SendAsync(new CreatePrescriptionCommand
            {
                MedicalRecordId = request.MedicalRecordId,
                IssueDate = request.IssueDate,
                Instruction = request.Instruction,
            }, cancellationToken);

            var response = ApiResponse<PrescriptionResponse>.Success(result, "Prescription created.", StatusCodes.Status201Created);
            return StatusCode(StatusCodes.Status201Created, response);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponse<PrescriptionResponse>>> Update(int id, UpdatePrescriptionRequest request, CancellationToken cancellationToken)
        {
            var result = await _mediator.SendAsync(new UpdatePrescriptionCommand
            {
                PrescriptionId = id,
                IssueDate = request.IssueDate,
                Instruction = request.Instruction,
            }, cancellationToken);

            return Ok(ApiResponse<PrescriptionResponse>.Success(result, "Prescription updated."));
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponse>> Delete(int id, CancellationToken cancellationToken)
        {
            await _mediator.SendAsync(new DeletePrescriptionRequest { PrescriptionId = id }, cancellationToken);
            return Ok(ApiResponse.Success("Prescription deleted."));
        }
    }
}
