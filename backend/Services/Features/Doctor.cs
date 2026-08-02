using backend.dbContext;
using backend.Exceptions;
using backend.Mediator.Interfaces;
using backend.Models;
using backend.Models.Dtos;
using backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Services.Features
{
    public class GetAllDoctorRequest : IRequest<IEnumerable<DoctorResponse>>
    {
    }

    public class GetAllDoctorHandler : IRequestHandler<GetAllDoctorRequest, IEnumerable<DoctorResponse>>
    {
        private readonly IDoctorRepository _doctorRepository;

        public GetAllDoctorHandler(IDoctorRepository doctorRepository)
        {
            _doctorRepository = doctorRepository;
        }

        public async Task<IEnumerable<DoctorResponse>> HandleAsync(GetAllDoctorRequest request, CancellationToken cancellationToken)
        {
            var doctors = await _doctorRepository.GetAllAsync();
            return doctors.Select(DoctorResponse.FromEntity);
        }
    }

    public class GetDoctorByIdRequest : IRequest<DoctorResponse>
    {
        public required int DoctorId { get; init; }
    }

    public class GetDoctorByIdHandler : IRequestHandler<GetDoctorByIdRequest, DoctorResponse>
    {
        private readonly IDoctorRepository _doctorRepository;

        public GetDoctorByIdHandler(IDoctorRepository doctorRepository)
        {
            _doctorRepository = doctorRepository;
        }

        public async Task<DoctorResponse> HandleAsync(GetDoctorByIdRequest request, CancellationToken cancellationToken)
        {
            var doctor = await _doctorRepository.GetByIdAsync(request.DoctorId)
                ?? throw new KeyNotFoundException($"Doctor {request.DoctorId} not found.");

            return DoctorResponse.FromEntity(doctor);
        }
    }

    public class UpdateDoctorRequest : IRequest<DoctorResponse>
    {
        public required int DoctorId { get; init; }
        public required DoctorUpdateDetails Details { get; init; }
        public required int RequestingUserId { get; init; }
        public required Role RequestingUserRole { get; init; }
    }

    public class UpdateDoctorHandler : IRequestHandler<UpdateDoctorRequest, DoctorResponse>
    {
        private readonly IDoctorRepository _doctorRepository;
        private readonly ApplicationDbContext _db;

        public UpdateDoctorHandler(IDoctorRepository doctorRepository, ApplicationDbContext db)
        {
            _doctorRepository = doctorRepository;
            _db = db;
        }

        public async Task<DoctorResponse> HandleAsync(UpdateDoctorRequest request, CancellationToken cancellationToken)
        {
            var doctor = await _doctorRepository.GetByIdAsync(request.DoctorId)
                ?? throw new KeyNotFoundException($"Doctor {request.DoctorId} not found.");

            if (request.RequestingUserRole == Role.DOCTOR && doctor.UserId != request.RequestingUserId)
            {
                throw new ForbiddenAccessException("You can only update your own doctor record.");
            }

            var details = request.Details;
            var departmentName = details.DepartmentName.Trim();
            var department = await _db.Departments.FirstOrDefaultAsync(d => d.Name == departmentName, cancellationToken)
                ?? new Department { Name = departmentName };

            doctor.FName = details.FName;
            doctor.LName = details.LName;
            doctor.DoB = details.DoB;
            doctor.Gender = details.Gender;
            doctor.Address = details.Address;
            doctor.Phone = details.Phone;
            doctor.Email = details.Email;
            doctor.LicenseNumber = details.LicenseNumber;
            doctor.Specialization = details.Specialization;
            doctor.ConsulationFee = details.ConsulationFee;
            doctor.Department = department;

            _doctorRepository.Update(doctor);
            await _doctorRepository.SaveChangesAsync();

            return DoctorResponse.FromEntity(doctor);
        }
    }

    public class DeleteDoctorRequest : IRequest<bool>
    {
        public required int DoctorId { get; init; }
    }

    public class DeleteDoctorHandler : IRequestHandler<DeleteDoctorRequest, bool>
    {
        private readonly IDoctorRepository _doctorRepository;

        public DeleteDoctorHandler(IDoctorRepository doctorRepository)
        {
            _doctorRepository = doctorRepository;
        }

        public async Task<bool> HandleAsync(DeleteDoctorRequest request, CancellationToken cancellationToken)
        {
            var doctor = await _doctorRepository.GetByIdAsync(request.DoctorId)
                ?? throw new KeyNotFoundException($"Doctor {request.DoctorId} not found.");

            _doctorRepository.Remove(doctor);
            await _doctorRepository.SaveChangesAsync();

            return true;
        }
    }

    public class UpdateDoctorStatusRequest : IRequest<DoctorResponse>
    {
        public required int DoctorId { get; init; }
        public required Status Status { get; init; }
    }

    public class UpdateDoctorStatusHandler : IRequestHandler<UpdateDoctorStatusRequest, DoctorResponse>
    {
        private readonly IDoctorRepository _doctorRepository;

        public UpdateDoctorStatusHandler(IDoctorRepository doctorRepository)
        {
            _doctorRepository = doctorRepository;
        }

        public async Task<DoctorResponse> HandleAsync(UpdateDoctorStatusRequest request, CancellationToken cancellationToken)
        {
            var doctor = await _doctorRepository.GetByIdAsync(request.DoctorId)
                ?? throw new KeyNotFoundException($"Doctor {request.DoctorId} not found.");

            doctor.status = request.Status switch
            {
                Status.ACTIVE => Status.ACTIVE,
                Status.INACTIVE => Status.INACTIVE,
                _ => throw new ArgumentException("Status must be either ACTIVE or INACTIVE."),
            };

            _doctorRepository.Update(doctor);
            await _doctorRepository.SaveChangesAsync();

            return DoctorResponse.FromEntity(doctor);
        }
    }
}
