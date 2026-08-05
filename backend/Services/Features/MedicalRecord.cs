using backend.Mediator.Interfaces;
using backend.Models;
using backend.Models.Dtos;
using backend.Repositories.Interfaces;

namespace backend.Services.Features
{
    public class GetAllMedicalRecordRequest : IRequest<IEnumerable<MedicalRecordResponse>>
    {
    }

    public class GetAllMedicalRecordHandler : IRequestHandler<GetAllMedicalRecordRequest, IEnumerable<MedicalRecordResponse>>
    {
        private readonly IMedicalRecordRepository _medicalRecordRepository;

        public GetAllMedicalRecordHandler(IMedicalRecordRepository medicalRecordRepository)
        {
            _medicalRecordRepository = medicalRecordRepository;
        }

        public async Task<IEnumerable<MedicalRecordResponse>> HandleAsync(GetAllMedicalRecordRequest request, CancellationToken cancellationToken)
        {
            var records = await _medicalRecordRepository.GetAllAsync();
            return records.Select(MedicalRecordResponse.FromEntity);
        }
    }

    public class GetMyMedicalRecordsRequest : IRequest<IEnumerable<MedicalRecordResponse>>
    {
        public required int RequestingUserId { get; init; }
    }

    public class GetMyMedicalRecordsHandler : IRequestHandler<GetMyMedicalRecordsRequest, IEnumerable<MedicalRecordResponse>>
    {
        private readonly IMedicalRecordRepository _medicalRecordRepository;
        private readonly IPatientRepository _patientRepository;

        public GetMyMedicalRecordsHandler(IMedicalRecordRepository medicalRecordRepository, IPatientRepository patientRepository)
        {
            _medicalRecordRepository = medicalRecordRepository;
            _patientRepository = patientRepository;
        }

        public async Task<IEnumerable<MedicalRecordResponse>> HandleAsync(GetMyMedicalRecordsRequest request, CancellationToken cancellationToken)
        {
            var patient = await _patientRepository.GetByUserIdAsync(request.RequestingUserId)
                ?? throw new KeyNotFoundException("Patient profile not found for the current user.");

            var records = await _medicalRecordRepository.GetAllAsync();
            return records.Where(record => record.PatientId == patient.Id).Select(MedicalRecordResponse.FromEntity);
        }
    }

    public class GetMedicalRecordByIdRequest : IRequest<MedicalRecordResponse>
    {
        public required int MedicalRecordId { get; init; }
    }

    public class GetMedicalRecordByIdHandler : IRequestHandler<GetMedicalRecordByIdRequest, MedicalRecordResponse>
    {
        private readonly IMedicalRecordRepository _medicalRecordRepository;

        public GetMedicalRecordByIdHandler(IMedicalRecordRepository medicalRecordRepository)
        {
            _medicalRecordRepository = medicalRecordRepository;
        }

        public async Task<MedicalRecordResponse> HandleAsync(GetMedicalRecordByIdRequest request, CancellationToken cancellationToken)
        {
            var record = await _medicalRecordRepository.GetByIdAsync(request.MedicalRecordId)
                ?? throw new KeyNotFoundException($"MedicalRecord {request.MedicalRecordId} not found.");

            return MedicalRecordResponse.FromEntity(record);
        }
    }

    public class CreateMedicalRecordCommand : IRequest<MedicalRecordResponse>
    {
        public required int PatientId { get; init; }
        public int? AppointmentId { get; init; }
        public required DateTime Visit { get; init; }
        public required string Diagnosis { get; init; }
        public string? Notes { get; init; }
        public required int RequestingUserId { get; init; }
    }

    public class CreateMedicalRecordHandler : IRequestHandler<CreateMedicalRecordCommand, MedicalRecordResponse>
    {
        private readonly IMedicalRecordRepository _medicalRecordRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IAppointmentRepository _appointmentRepository;

        public CreateMedicalRecordHandler(
            IMedicalRecordRepository medicalRecordRepository,
            IDoctorRepository doctorRepository,
            IPatientRepository patientRepository,
            IAppointmentRepository appointmentRepository)
        {
            _medicalRecordRepository = medicalRecordRepository;
            _doctorRepository = doctorRepository;
            _patientRepository = patientRepository;
            _appointmentRepository = appointmentRepository;
        }

        public async Task<MedicalRecordResponse> HandleAsync(CreateMedicalRecordCommand request, CancellationToken cancellationToken)
        {
            var doctor = await _doctorRepository.GetByUserIdAsync(request.RequestingUserId)
                ?? throw new KeyNotFoundException("Doctor profile not found for the current user.");

            _ = await _patientRepository.GetByIdAsync(request.PatientId)
                ?? throw new KeyNotFoundException($"Patient {request.PatientId} not found.");

            if (request.AppointmentId is not null)
            {
                var appointment = await _appointmentRepository.GetByIdAsync(request.AppointmentId.Value)
                    ?? throw new KeyNotFoundException($"Appointment {request.AppointmentId} not found.");

                if (appointment.DoctorId != doctor.Id || appointment.PatientId != request.PatientId)
                {
                    throw new ArgumentException("The appointment does not belong to this doctor/patient pair.");
                }

                if (appointment.MedicalRecord is not null)
                {
                    throw new InvalidOperationException("This appointment already has a medical record.");
                }
            }

            var record = new MedicalRecord
            {
                DoctorId = doctor.Id,
                PatientId = request.PatientId,
                AppointmentId = request.AppointmentId,
                Visit = request.Visit,
                Diagnosis = request.Diagnosis,
                Notes = request.Notes,
            };

            await _medicalRecordRepository.AddAsync(record);
            await _medicalRecordRepository.SaveChangesAsync();

            var created = await _medicalRecordRepository.GetByIdAsync(record.Id)
                ?? throw new KeyNotFoundException("MedicalRecord not found after creation.");

            return MedicalRecordResponse.FromEntity(created);
        }
    }

    public class UpdateMedicalRecordCommand : IRequest<MedicalRecordResponse>
    {
        public required int MedicalRecordId { get; init; }
        public required DateTime Visit { get; init; }
        public required string Diagnosis { get; init; }
        public string? Notes { get; init; }
    }

    public class UpdateMedicalRecordHandler : IRequestHandler<UpdateMedicalRecordCommand, MedicalRecordResponse>
    {
        private readonly IMedicalRecordRepository _medicalRecordRepository;

        public UpdateMedicalRecordHandler(IMedicalRecordRepository medicalRecordRepository)
        {
            _medicalRecordRepository = medicalRecordRepository;
        }

        public async Task<MedicalRecordResponse> HandleAsync(UpdateMedicalRecordCommand request, CancellationToken cancellationToken)
        {
            var record = await _medicalRecordRepository.GetByIdAsync(request.MedicalRecordId)
                ?? throw new KeyNotFoundException($"MedicalRecord {request.MedicalRecordId} not found.");

            record.Visit = request.Visit;
            record.Diagnosis = request.Diagnosis;
            record.Notes = request.Notes;

            _medicalRecordRepository.Update(record);
            await _medicalRecordRepository.SaveChangesAsync();

            return MedicalRecordResponse.FromEntity(record);
        }
    }

    public class DeleteMedicalRecordRequest : IRequest<bool>
    {
        public required int MedicalRecordId { get; init; }
    }

    public class DeleteMedicalRecordHandler : IRequestHandler<DeleteMedicalRecordRequest, bool>
    {
        private readonly IMedicalRecordRepository _medicalRecordRepository;
        private readonly IMedicineRepository _medicineRepository;

        public DeleteMedicalRecordHandler(IMedicalRecordRepository medicalRecordRepository, IMedicineRepository medicineRepository)
        {
            _medicalRecordRepository = medicalRecordRepository;
            _medicineRepository = medicineRepository;
        }

        public async Task<bool> HandleAsync(DeleteMedicalRecordRequest request, CancellationToken cancellationToken)
        {
            var record = await _medicalRecordRepository.GetByIdAsync(request.MedicalRecordId)
                ?? throw new KeyNotFoundException($"MedicalRecord {request.MedicalRecordId} not found.");

            // Deleting a medical record cascades to its prescriptions and their items; restore the stock they consumed first.
            foreach (var item in record.Prescriptions.SelectMany(prescription => prescription.PrescriptionItems))
            {
                var medicine = await _medicineRepository.GetByIdAsync(item.MedicineId);
                if (medicine is not null)
                {
                    medicine.StockQt += item.Quantity;
                    _medicineRepository.Update(medicine);
                }
            }

            _medicalRecordRepository.Remove(record);
            await _medicalRecordRepository.SaveChangesAsync();

            return true;
        }
    }
}
