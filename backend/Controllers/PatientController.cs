using backend.Mediator.Interfaces;
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
        public async Task<ActionResult<IEnumerable<PatientResponse>>> GetAll(CancellationToken cancellationToken)
        {
            var result = await _mediator.SendAsync(new GetAllPatientRequest(), cancellationToken);
            return Ok(result);
        }
    }
}
