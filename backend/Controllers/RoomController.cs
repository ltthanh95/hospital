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
    public class RoomController : ControllerBase
    {
        private readonly IMyMediator _mediator;

        public RoomController(IMyMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<RoomResponse>>>> GetAll(CancellationToken cancellationToken)
        {
            var result = await _mediator.SendAsync(new GetAllRoomRequest(), cancellationToken);
            return Ok(ApiResponse<IEnumerable<RoomResponse>>.Success(result));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<RoomResponse>>> GetById(int id, CancellationToken cancellationToken)
        {
            var result = await _mediator.SendAsync(new GetRoomByIdRequest { RoomId = id }, cancellationToken);
            return Ok(ApiResponse<RoomResponse>.Success(result));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<RoomResponse>>> Create(RoomRequest request, CancellationToken cancellationToken)
        {
            var result = await _mediator.SendAsync(new CreateRoomCommand
            {
                RoomNumber = request.RoomNumber,
                Type = request.Type,
                Capacity = request.Capacity,
            }, cancellationToken);

            var response = ApiResponse<RoomResponse>.Success(result, "Room created.", StatusCodes.Status201Created);
            return StatusCode(StatusCodes.Status201Created, response);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponse<RoomResponse>>> Update(int id, RoomRequest request, CancellationToken cancellationToken)
        {
            var result = await _mediator.SendAsync(new UpdateRoomCommand
            {
                RoomId = id,
                RoomNumber = request.RoomNumber,
                Type = request.Type,
                Capacity = request.Capacity,
            }, cancellationToken);

            return Ok(ApiResponse<RoomResponse>.Success(result, "Room updated."));
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponse>> Delete(int id, CancellationToken cancellationToken)
        {
            await _mediator.SendAsync(new DeleteRoomRequest { RoomId = id }, cancellationToken);
            return Ok(ApiResponse.Success("Room deleted."));
        }
    }
}
