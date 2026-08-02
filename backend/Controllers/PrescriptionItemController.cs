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
    public class PrescriptionItemController : ControllerBase
    {
        private readonly IMyMediator _mediator;

        public PrescriptionItemController(IMyMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<PrescriptionItemResponse>>>> GetAll(CancellationToken cancellationToken)
        {
            var result = await _mediator.SendAsync(new GetAllPrescriptionItemRequest(), cancellationToken);
            return Ok(ApiResponse<IEnumerable<PrescriptionItemResponse>>.Success(result));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<PrescriptionItemResponse>>> GetById(int id, CancellationToken cancellationToken)
        {
            var result = await _mediator.SendAsync(new GetPrescriptionItemByIdRequest { PrescriptionItemId = id }, cancellationToken);
            return Ok(ApiResponse<PrescriptionItemResponse>.Success(result));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<PrescriptionItemResponse>>> Create(CreatePrescriptionItemRequest request, CancellationToken cancellationToken)
        {
            var result = await _mediator.SendAsync(new CreatePrescriptionItemCommand
            {
                PrescriptionId = request.PrescriptionId,
                MedicineId = request.MedicineId,
                Dosage = request.Dosage,
                Quantity = request.Quantity,
                Frequency = request.Frequency,
                DurationDays = request.DurationDays,
            }, cancellationToken);

            var response = ApiResponse<PrescriptionItemResponse>.Success(result, "Prescription item created.", StatusCodes.Status201Created);
            return StatusCode(StatusCodes.Status201Created, response);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponse<PrescriptionItemResponse>>> Update(int id, UpdatePrescriptionItemRequest request, CancellationToken cancellationToken)
        {
            var result = await _mediator.SendAsync(new UpdatePrescriptionItemCommand
            {
                PrescriptionItemId = id,
                MedicineId = request.MedicineId,
                Dosage = request.Dosage,
                Quantity = request.Quantity,
                Frequency = request.Frequency,
                DurationDays = request.DurationDays,
            }, cancellationToken);

            return Ok(ApiResponse<PrescriptionItemResponse>.Success(result, "Prescription item updated."));
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponse>> Delete(int id, CancellationToken cancellationToken)
        {
            await _mediator.SendAsync(new DeletePrescriptionItemRequest { PrescriptionItemId = id }, cancellationToken);
            return Ok(ApiResponse.Success("Prescription item deleted."));
        }
    }
}
