using backend.Exceptions;
using backend.Mediator.Interfaces;
using backend.Models;
using backend.Models.Dtos;
using backend.Repositories.Interfaces;

namespace backend.Services.Features
{
    public class GetAllAppointmentRequest : IRequest<IEnumerable<AppointmentResponse>>
    {
    }

    public class GetAllAppointmentHandler : IRequestHandler<GetAllAppointmentRequest, IEnumerable<AppointmentResponse>>
    {
        private readonly IAppointmentRepository _appointmentRepository;

        public GetAllAppointmentHandler(IAppointmentRepository appointmentRepository)
        {
            _appointmentRepository = appointmentRepository;
        }

        public async Task<IEnumerable<AppointmentResponse>> HandleAsync(GetAllAppointmentRequest request, CancellationToken cancellationToken)
        {
            var appointments = await _appointmentRepository.GetAllAsync();
            return appointments.Select(AppointmentResponse.FromEntity);
        }
    }

    public class GetAppointmentByIdRequest : IRequest<AppointmentResponse>
    {
        public required int AppointmentId { get; init; }
    }

    public class GetAppointmentByIdHandler : IRequestHandler<GetAppointmentByIdRequest, AppointmentResponse>
    {
        private readonly IAppointmentRepository _appointmentRepository;

        public GetAppointmentByIdHandler(IAppointmentRepository appointmentRepository)
        {
            _appointmentRepository = appointmentRepository;
        }

        public async Task<AppointmentResponse> HandleAsync(GetAppointmentByIdRequest request, CancellationToken cancellationToken)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(request.AppointmentId)
                ?? throw new KeyNotFoundException($"Appointment {request.AppointmentId} not found.");

            return AppointmentResponse.FromEntity(appointment);
        }
    }

    public class CreateAppointmentCommand : IRequest<AppointmentResponse>
    {
        public int? PatientId { get; init; }
        public int? DoctorId { get; init; }
        public required DateTime Schedule { get; init; }
        public required string Reason { get; init; }
        public required int RequestingUserId { get; init; }
        public required Role RequestingUserRole { get; init; }
    }

    public class CreateAppointmentHandler : IRequestHandler<CreateAppointmentCommand, AppointmentResponse>
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IDoctorRepository _doctorRepository;

        public CreateAppointmentHandler(
            IAppointmentRepository appointmentRepository,
            IPatientRepository patientRepository,
            IDoctorRepository doctorRepository)
        {
            _appointmentRepository = appointmentRepository;
            _patientRepository = patientRepository;
            _doctorRepository = doctorRepository;
        }

        public async Task<AppointmentResponse> HandleAsync(CreateAppointmentCommand request, CancellationToken cancellationToken)
        {
            if (request.Schedule <= DateTime.UtcNow)
            {
                throw new ArgumentException("Appointment schedule must be in the future.");
            }

            int patientId;
            int doctorId;

            if (request.RequestingUserRole == Role.PATIENT)
            {
                var patient = await _patientRepository.GetByUserIdAsync(request.RequestingUserId)
                    ?? throw new KeyNotFoundException("Patient profile not found for the current user.");

                if (request.DoctorId is null)
                {
                    throw new ArgumentException("DoctorId is required when a patient creates an appointment.");
                }

                patientId = patient.Id;
                doctorId = request.DoctorId.Value;

                _ = await _doctorRepository.GetByIdAsync(doctorId)
                    ?? throw new KeyNotFoundException($"Doctor {doctorId} not found.");
            }
            else if (request.RequestingUserRole == Role.DOCTOR)
            {
                var doctor = await _doctorRepository.GetByUserIdAsync(request.RequestingUserId)
                    ?? throw new KeyNotFoundException("Doctor profile not found for the current user.");

                if (request.PatientId is null)
                {
                    throw new ArgumentException("PatientId is required when a doctor creates an appointment.");
                }

                doctorId = doctor.Id;
                patientId = request.PatientId.Value;

                _ = await _patientRepository.GetByIdAsync(patientId)
                    ?? throw new KeyNotFoundException($"Patient {patientId} not found.");
            }
            else
            {
                throw new ForbiddenAccessException("Only patients and doctors can create appointments.");
            }

            var hasConflict = await _appointmentRepository.DoctorHasConflictAsync(doctorId, request.Schedule);

            var appointment = new Appointment
            {
                Schedule = request.Schedule,
                Reason = request.Reason,
                DoctorId = doctorId,
                PatientId = patientId,
                Status = hasConflict ? AppointmentStatus.PENDING : AppointmentStatus.CONFIRMED,
            };

            await _appointmentRepository.AddAsync(appointment);
            await _appointmentRepository.SaveChangesAsync();

            var created = await _appointmentRepository.GetByIdAsync(appointment.Id)
                ?? throw new KeyNotFoundException("Appointment not found after creation.");

            return AppointmentResponse.FromEntity(created);
        }
    }

    public class RescheduleAppointmentCommand : IRequest<AppointmentResponse>
    {
        public required int AppointmentId { get; init; }
        public required DateTime Schedule { get; init; }
        public required int RequestingUserId { get; init; }
    }

    public class RescheduleAppointmentHandler : IRequestHandler<RescheduleAppointmentCommand, AppointmentResponse>
    {
        private readonly IAppointmentRepository _appointmentRepository;

        public RescheduleAppointmentHandler(IAppointmentRepository appointmentRepository)
        {
            _appointmentRepository = appointmentRepository;
        }

        public async Task<AppointmentResponse> HandleAsync(RescheduleAppointmentCommand request, CancellationToken cancellationToken)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(request.AppointmentId)
                ?? throw new KeyNotFoundException($"Appointment {request.AppointmentId} not found.");

            if (appointment.Patient.UserId != request.RequestingUserId)
            {
                throw new ForbiddenAccessException("You can only reschedule your own appointment.");
            }

            if (appointment.Status == AppointmentStatus.CANCELLED)
            {
                throw new InvalidOperationException("A cancelled appointment cannot be rescheduled.");
            }

            if (request.Schedule <= DateTime.UtcNow)
            {
                throw new ArgumentException("Appointment schedule must be in the future.");
            }

            appointment.Schedule = request.Schedule;

            var hasConflict = await _appointmentRepository.DoctorHasConflictAsync(
                appointment.DoctorId, request.Schedule, appointment.Id);
            appointment.Status = hasConflict ? AppointmentStatus.PENDING : AppointmentStatus.CONFIRMED;

            _appointmentRepository.Update(appointment);
            await _appointmentRepository.SaveChangesAsync();

            return AppointmentResponse.FromEntity(appointment);
        }
    }

    public class CancelAppointmentCommand : IRequest<AppointmentResponse>
    {
        public required int AppointmentId { get; init; }
        public required int RequestingUserId { get; init; }
    }

    public class CancelAppointmentHandler : IRequestHandler<CancelAppointmentCommand, AppointmentResponse>
    {
        private readonly IAppointmentRepository _appointmentRepository;

        public CancelAppointmentHandler(IAppointmentRepository appointmentRepository)
        {
            _appointmentRepository = appointmentRepository;
        }

        public async Task<AppointmentResponse> HandleAsync(CancelAppointmentCommand request, CancellationToken cancellationToken)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(request.AppointmentId)
                ?? throw new KeyNotFoundException($"Appointment {request.AppointmentId} not found.");

            if (appointment.Patient.UserId != request.RequestingUserId)
            {
                throw new ForbiddenAccessException("You can only cancel your own appointment.");
            }

            appointment.Status = AppointmentStatus.CANCELLED;

            _appointmentRepository.Update(appointment);
            await _appointmentRepository.SaveChangesAsync();

            return AppointmentResponse.FromEntity(appointment);
        }
    }
}
