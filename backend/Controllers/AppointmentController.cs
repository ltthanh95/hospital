using System.Security.Claims;
using backend.Mediator.Interfaces;
using backend.Models;
using backend.Models.Dtos;
using backend.Services.Features;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/*
 * handles incoming HTTP requests, processes inputs, interacts with services, and returns the appropriate response
*/

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //Authorized user can access to this controller
    [Authorize]
    public class AppointmentController : ControllerBase
    {
        //Inject Mediator to controller to call method and connect to funcionalities' request from service
        private readonly IMyMediator _mediator;

        public AppointmentController(IMyMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<AppointmentResponse>>>> GetAll(CancellationToken cancellationToken)
        {
            //Send mediator to call request
            var result = await _mediator.SendAsync(new GetAllAppointmentRequest(), cancellationToken);
            return Ok(ApiResponse<IEnumerable<AppointmentResponse>>.Success(result));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<AppointmentResponse>>> GetById(int id, CancellationToken cancellationToken)
        {
            //Send mediator to call request
            var result = await _mediator.SendAsync(new GetAppointmentByIdRequest { AppointmentId = id }, cancellationToken);
            return Ok(ApiResponse<AppointmentResponse>.Success(result));
        }

        [HttpPost]
        [Authorize(Roles = $"{nameof(Role.PATIENT)},{nameof(Role.DOCTOR)}")]
        public async Task<ActionResult<ApiResponse<AppointmentResponse>>> Create(CreateAppointmentRequest request, CancellationToken cancellationToken)
        {
            //Get current user to check authentication and check current  user's appointment
            var (userId, role) = GetCurrentUser();
            //Send mediator to call request with parameters
            var result = await _mediator.SendAsync(new CreateAppointmentCommand
            {
                PatientId = request.PatientId,
                DoctorId = request.DoctorId,
                Schedule = request.Schedule,
                Reason = request.Reason,
                RequestingUserId = userId,
                RequestingUserRole = role,
            }, cancellationToken);

            var response = ApiResponse<AppointmentResponse>.Success(result, "Appointment created.", StatusCodes.Status201Created);
            return StatusCode(StatusCodes.Status201Created, response);
        }

        [HttpPut("{id:int}/reschedule")]
        [Authorize(Roles = nameof(Role.PATIENT))]
        public async Task<ActionResult<ApiResponse<AppointmentResponse>>> Reschedule(int id, RescheduleAppointmentRequest request, CancellationToken cancellationToken)
        {
            var (userId, _) = GetCurrentUser();
            //Send mediator to call request
            var result = await _mediator.SendAsync(new RescheduleAppointmentCommand
            {
                AppointmentId = id,
                Schedule = request.Schedule,
                RequestingUserId = userId,
            }, cancellationToken);

            return Ok(ApiResponse<AppointmentResponse>.Success(result, "Appointment rescheduled."));
        }

        [HttpPatch("{id:int}/cancel")]
        [Authorize(Roles = nameof(Role.PATIENT))]
        public async Task<ActionResult<ApiResponse<AppointmentResponse>>> Cancel(int id, CancellationToken cancellationToken)
        {
            var (userId, _) = GetCurrentUser();
            var result = await _mediator.SendAsync(new CancelAppointmentCommand
            {
                AppointmentId = id,
                RequestingUserId = userId,
            }, cancellationToken);

            return Ok(ApiResponse<AppointmentResponse>.Success(result, "Appointment cancelled."));
        }

        //Get current user for authentication => Example: I login to the page, I can only see my current page, cannot see anyone else's page
        private (int UserId, Role Role) GetCurrentUser()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")
                ?? throw new UnauthorizedAccessException("User id claim is missing.");
            var roleClaim = User.FindFirstValue(ClaimTypes.Role)
                ?? throw new UnauthorizedAccessException("Role claim is missing.");

            return (int.Parse(userIdClaim), Enum.Parse<Role>(roleClaim));
        }
    }
}
