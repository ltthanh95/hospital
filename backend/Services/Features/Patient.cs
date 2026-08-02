using backend.Exceptions;
using backend.Mediator.Interfaces;
using backend.Models;
using backend.Models.Dtos;
using backend.Repositories.Interfaces;

namespace backend.Services.Features
{
    public class GetAllPatientRequest : IRequest<IEnumerable<PatientResponse>>
    {
    }

    public class GetAllPatientHandler : IRequestHandler<GetAllPatientRequest, IEnumerable<PatientResponse>>
    {
        private readonly IPatientRepository _patientRepository;

        public GetAllPatientHandler(IPatientRepository patientRepository)
        {
            _patientRepository = patientRepository;
        }

        public async Task<IEnumerable<PatientResponse>> HandleAsync(GetAllPatientRequest request, CancellationToken cancellationToken)
        {
            var patients = await _patientRepository.GetAllAsync();
            return patients.Select(PatientResponse.FromEntity);
        }
    }

    public class GetPatientByIdRequest : IRequest<PatientResponse>
    {
        public required int PatientId { get; init; }
    }

    public class GetPatientByIdHandler : IRequestHandler<GetPatientByIdRequest, PatientResponse>
    {
        private readonly IPatientRepository _patientRepository;

        public GetPatientByIdHandler(IPatientRepository patientRepository)
        {
            _patientRepository = patientRepository;
        }

        public async Task<PatientResponse> HandleAsync(GetPatientByIdRequest request, CancellationToken cancellationToken)
        {
            var patient = await _patientRepository.GetByIdAsync(request.PatientId)
                ?? throw new KeyNotFoundException($"Patient {request.PatientId} not found.");

            return PatientResponse.FromEntity(patient);
        }
    }

    public class UpdatePatientRequest : IRequest<PatientResponse>
    {
        public required int PatientId { get; init; }
        public required PatientUpdateDetails Details { get; init; }
        public required int RequestingUserId { get; init; }
        public required Role RequestingUserRole { get; init; }
    }

    public class UpdatePatientHandler : IRequestHandler<UpdatePatientRequest, PatientResponse>
    {
        private readonly IPatientRepository _patientRepository;

        public UpdatePatientHandler(IPatientRepository patientRepository)
        {
            _patientRepository = patientRepository;
        }

        public async Task<PatientResponse> HandleAsync(UpdatePatientRequest request, CancellationToken cancellationToken)
        {
            var patient = await _patientRepository.GetByIdAsync(request.PatientId)
                ?? throw new KeyNotFoundException($"Patient {request.PatientId} not found.");

            if (request.RequestingUserRole == Role.PATIENT && patient.UserId != request.RequestingUserId)
            {
                throw new ForbiddenAccessException("You can only update your own patient record.");
            }

            var details = request.Details;
            patient.FName = details.FName;
            patient.LName = details.LName;
            patient.DoB = details.DoB;
            patient.Gender = details.Gender;
            patient.Address = details.Address;
            patient.Phone = details.Phone;
            patient.Email = details.Email;
            patient.BloodType = details.BloodType;
            patient.EmergencyContact = details.EmergencyContact;

            _patientRepository.Update(patient);
            await _patientRepository.SaveChangesAsync();

            return PatientResponse.FromEntity(patient);
        }
    }

    public class DeletePatientRequest : IRequest<bool>
    {
        public required int PatientId { get; init; }
    }

    public class DeletePatientHandler : IRequestHandler<DeletePatientRequest, bool>
    {
        private readonly IPatientRepository _patientRepository;

        public DeletePatientHandler(IPatientRepository patientRepository)
        {
            _patientRepository = patientRepository;
        }

        public async Task<bool> HandleAsync(DeletePatientRequest request, CancellationToken cancellationToken)
        {
            var patient = await _patientRepository.GetByIdAsync(request.PatientId)
                ?? throw new KeyNotFoundException($"Patient {request.PatientId} not found.");

            _patientRepository.Remove(patient);
            await _patientRepository.SaveChangesAsync();

            return true;
        }
    }

    public class UpdatePatientStatusRequest : IRequest<PatientResponse>
    {
        public required int PatientId { get; init; }
        public required Status Status { get; init; }
    }

    public class UpdatePatientStatusHandler : IRequestHandler<UpdatePatientStatusRequest, PatientResponse>
    {
        private readonly IPatientRepository _patientRepository;

        public UpdatePatientStatusHandler(IPatientRepository patientRepository)
        {
            _patientRepository = patientRepository;
        }

        public async Task<PatientResponse> HandleAsync(UpdatePatientStatusRequest request, CancellationToken cancellationToken)
        {
            var patient = await _patientRepository.GetByIdAsync(request.PatientId)
                ?? throw new KeyNotFoundException($"Patient {request.PatientId} not found.");

            switch (request.Status)
            {
                case Status.ADMISSION:
                    patient.Admission();
                    patient.DischargeDate = null;
                    break;
                case Status.DISCHARGE:
                    patient.Discharge();
                    patient.DischargeDate = DateTime.UtcNow;
                    break;
                default:
                    throw new ArgumentException("Status must be either ADMISSION or DISCHARGE.");
            }

            _patientRepository.Update(patient);
            await _patientRepository.SaveChangesAsync();

            return PatientResponse.FromEntity(patient);
        }
    }
}
