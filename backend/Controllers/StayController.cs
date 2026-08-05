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
    public class StayController : ControllerBase
    {
        private readonly IMyMediator _mediator;

        public StayController(IMyMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Authorize(Roles = $"{nameof(Role.ADMIN)},{nameof(Role.DOCTOR)}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<StayResponse>>>> GetAll(CancellationToken cancellationToken)
        {
            var result = await _mediator.SendAsync(new GetAllStayRequest(), cancellationToken);
            return Ok(ApiResponse<IEnumerable<StayResponse>>.Success(result));
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = $"{nameof(Role.ADMIN)},{nameof(Role.DOCTOR)}")]
        public async Task<ActionResult<ApiResponse<StayResponse>>> GetById(int id, CancellationToken cancellationToken)
        {
            var result = await _mediator.SendAsync(new GetStayByIdRequest { StayId = id }, cancellationToken);
            return Ok(ApiResponse<StayResponse>.Success(result));
        }

        [HttpPost]
        [Authorize(Roles = $"{nameof(Role.ADMIN)},{nameof(Role.DOCTOR)}")]
        public async Task<ActionResult<ApiResponse<StayResponse>>> Create(CreateStayRequest request, CancellationToken cancellationToken)
        {
            var result = await _mediator.SendAsync(new CreateStayCommand
            {
                PatientId = request.PatientId,
                RoomId = request.RoomId,
                CheckIn = request.CheckIn,
            }, cancellationToken);

            var response = ApiResponse<StayResponse>.Success(result, "Patient assigned to room.", StatusCodes.Status201Created);
            return StatusCode(StatusCodes.Status201Created, response);
        }

        [HttpPatch("{id:int}/checkout")]
        [Authorize(Roles = nameof(Role.ADMIN))]
        public async Task<ActionResult<ApiResponse<StayResponse>>> Checkout(int id, CancellationToken cancellationToken)
        {
            var result = await _mediator.SendAsync(new CheckoutStayRequest { StayId = id }, cancellationToken);
            return Ok(ApiResponse<StayResponse>.Success(result, "Patient checked out."));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = nameof(Role.ADMIN))]
        public async Task<ActionResult<ApiResponse>> Delete(int id, CancellationToken cancellationToken)
        {
            await _mediator.SendAsync(new DeleteStayRequest { StayId = id }, cancellationToken);
            return Ok(ApiResponse.Success("Stay deleted."));
        }
    }
}
