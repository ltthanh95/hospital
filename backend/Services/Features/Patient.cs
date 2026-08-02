using backend.Mediator.Interfaces;
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
}
