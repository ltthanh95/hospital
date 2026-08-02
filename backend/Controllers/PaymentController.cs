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
    [Authorize(Roles = nameof(Role.ADMIN))]
    public class PaymentController : ControllerBase
    {
        private readonly IMyMediator _mediator;

        public PaymentController(IMyMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<PaymentResponse>>>> GetAll(CancellationToken cancellationToken)
        {
            var result = await _mediator.SendAsync(new GetAllPaymentRequest(), cancellationToken);
            return Ok(ApiResponse<IEnumerable<PaymentResponse>>.Success(result));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<PaymentResponse>>> GetById(int id, CancellationToken cancellationToken)
        {
            var result = await _mediator.SendAsync(new GetPaymentByIdRequest { PaymentId = id }, cancellationToken);
            return Ok(ApiResponse<PaymentResponse>.Success(result));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<PaymentResponse>>> Create(CreatePaymentRequest request, CancellationToken cancellationToken)
        {
            var result = await _mediator.SendAsync(new CreatePaymentCommand
            {
                InvoiceId = request.InvoiceId,
                Amount = request.Amount,
                Method = request.Method,
            }, cancellationToken);

            var response = ApiResponse<PaymentResponse>.Success(result, "Payment recorded.", StatusCodes.Status201Created);
            return StatusCode(StatusCodes.Status201Created, response);
        }
    }
}
