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
    public class RevenueController : ControllerBase
    {
        private readonly IMyMediator _mediator;

        public RevenueController(IMyMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<RevenueResponse>>> Get(CancellationToken cancellationToken)
        {
            var result = await _mediator.SendAsync(new GetRevenueReportRequest(), cancellationToken);
            return Ok(ApiResponse<RevenueResponse>.Success(result));
        }
    }
}
