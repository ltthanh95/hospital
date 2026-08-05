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
    public class InvoiceController : ControllerBase
    {
        private readonly IMyMediator _mediator;

        public InvoiceController(IMyMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Authorize(Roles = nameof(Role.ADMIN))]
        public async Task<ActionResult<ApiResponse<IEnumerable<InvoiceResponse>>>> GetAll(CancellationToken cancellationToken)
        {
            var result = await _mediator.SendAsync(new GetAllInvoiceRequest(), cancellationToken);
            return Ok(ApiResponse<IEnumerable<InvoiceResponse>>.Success(result));
        }

        [HttpGet("me")]
        [Authorize(Roles = nameof(Role.PATIENT))]
        public async Task<ActionResult<ApiResponse<IEnumerable<InvoiceResponse>>>> GetMine(CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _mediator.SendAsync(new GetMyInvoicesRequest { RequestingUserId = userId }, cancellationToken);
            return Ok(ApiResponse<IEnumerable<InvoiceResponse>>.Success(result));
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = nameof(Role.ADMIN))]
        public async Task<ActionResult<ApiResponse<InvoiceResponse>>> GetById(int id, CancellationToken cancellationToken)
        {
            var result = await _mediator.SendAsync(new GetInvoiceByIdRequest { InvoiceId = id }, cancellationToken);
            return Ok(ApiResponse<InvoiceResponse>.Success(result));
        }

        [HttpPost("generate")]
        [Authorize(Roles = nameof(Role.ADMIN))]
        public async Task<ActionResult<ApiResponse<InvoiceResponse>>> Generate(GenerateInvoiceRequest request, CancellationToken cancellationToken)
        {
            var result = await _mediator.SendAsync(new GenerateInvoiceCommand { PatientId = request.PatientId }, cancellationToken);
            var response = ApiResponse<InvoiceResponse>.Success(result, "Invoice generated.", StatusCodes.Status201Created);
            return StatusCode(StatusCodes.Status201Created, response);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = nameof(Role.ADMIN))]
        public async Task<ActionResult<ApiResponse>> Delete(int id, CancellationToken cancellationToken)
        {
            await _mediator.SendAsync(new DeleteInvoiceRequest { InvoiceId = id }, cancellationToken);
            return Ok(ApiResponse.Success("Invoice deleted."));
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
