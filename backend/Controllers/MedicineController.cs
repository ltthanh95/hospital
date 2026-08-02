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
    public class MedicineController : ControllerBase
    {
        private readonly IMyMediator _mediator;

        public MedicineController(IMyMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<MedicineResponse>>>> GetAll(CancellationToken cancellationToken)
        {
            var result = await _mediator.SendAsync(new GetAllMedicineRequest(), cancellationToken);
            return Ok(ApiResponse<IEnumerable<MedicineResponse>>.Success(result));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<MedicineResponse>>> GetById(int id, CancellationToken cancellationToken)
        {
            var result = await _mediator.SendAsync(new GetMedicineByIdRequest { MedicineId = id }, cancellationToken);
            return Ok(ApiResponse<MedicineResponse>.Success(result));
        }

        [HttpPost]
        [Authorize(Roles = nameof(Role.ADMIN))]
        public async Task<ActionResult<ApiResponse<MedicineResponse>>> Create(MedicineRequest request, CancellationToken cancellationToken)
        {
            var result = await _mediator.SendAsync(new CreateMedicineCommand
            {
                Name = request.Name,
                Manufacturer = request.Manufacturer,
                UnitPrice = request.UnitPrice,
                StockQt = request.StockQt,
                Expiration = request.Expiration,
            }, cancellationToken);

            var response = ApiResponse<MedicineResponse>.Success(result, "Medicine created.", StatusCodes.Status201Created);
            return StatusCode(StatusCodes.Status201Created, response);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = nameof(Role.ADMIN))]
        public async Task<ActionResult<ApiResponse<MedicineResponse>>> Update(int id, MedicineRequest request, CancellationToken cancellationToken)
        {
            var result = await _mediator.SendAsync(new UpdateMedicineCommand
            {
                MedicineId = id,
                Name = request.Name,
                Manufacturer = request.Manufacturer,
                UnitPrice = request.UnitPrice,
                StockQt = request.StockQt,
                Expiration = request.Expiration,
            }, cancellationToken);

            return Ok(ApiResponse<MedicineResponse>.Success(result, "Medicine updated."));
        }

        [HttpPatch("{id:int}/stock")]
        [Authorize(Roles = $"{nameof(Role.ADMIN)},{nameof(Role.DOCTOR)}")]
        public async Task<ActionResult<ApiResponse<MedicineResponse>>> UpdateStock(int id, UpdateMedicineStockRequest request, CancellationToken cancellationToken)
        {
            var result = await _mediator.SendAsync(new UpdateMedicineStockCommand
            {
                MedicineId = id,
                StockQt = request.StockQt,
            }, cancellationToken);

            return Ok(ApiResponse<MedicineResponse>.Success(result, "Medicine stock updated."));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = nameof(Role.ADMIN))]
        public async Task<ActionResult<ApiResponse>> Delete(int id, CancellationToken cancellationToken)
        {
            await _mediator.SendAsync(new DeleteMedicineRequest { MedicineId = id }, cancellationToken);
            return Ok(ApiResponse.Success("Medicine deleted."));
        }
    }
}
