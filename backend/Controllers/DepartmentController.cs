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
    public class DepartmentController : ControllerBase
    {
        private readonly IMyMediator _mediator;

        public DepartmentController(IMyMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<DepartmentResponse>>>> GetAll(CancellationToken cancellationToken)
        {
            var result = await _mediator.SendAsync(new GetAllDepartmentRequest(), cancellationToken);
            return Ok(ApiResponse<IEnumerable<DepartmentResponse>>.Success(result));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<DepartmentResponse>>> GetById(int id, CancellationToken cancellationToken)
        {
            var result = await _mediator.SendAsync(new GetDepartmentByIdRequest { DepartmentId = id }, cancellationToken);
            return Ok(ApiResponse<DepartmentResponse>.Success(result));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<DepartmentResponse>>> Create(DepartmentRequest request, CancellationToken cancellationToken)
        {
            var result = await _mediator.SendAsync(new CreateDepartmentRequest { Name = request.Name }, cancellationToken);
            var response = ApiResponse<DepartmentResponse>.Success(result, "Department created.", StatusCodes.Status201Created);
            return StatusCode(StatusCodes.Status201Created, response);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponse<DepartmentResponse>>> Update(int id, DepartmentRequest request, CancellationToken cancellationToken)
        {
            var result = await _mediator.SendAsync(new UpdateDepartmentRequest { DepartmentId = id, Name = request.Name }, cancellationToken);
            return Ok(ApiResponse<DepartmentResponse>.Success(result, "Department updated."));
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponse>> Delete(int id, CancellationToken cancellationToken)
        {
            await _mediator.SendAsync(new DeleteDepartmentRequest { DepartmentId = id }, cancellationToken);
            return Ok(ApiResponse.Success("Department deleted."));
        }
    }
}
