using backend.Mediator.Interfaces;
using backend.Models;
using backend.Models.Dtos;
using backend.Repositories.Interfaces;

namespace backend.Services.Features
{
    public class GetAllDepartmentRequest : IRequest<IEnumerable<DepartmentResponse>>
    {
    }

    public class GetAllDepartmentHandler : IRequestHandler<GetAllDepartmentRequest, IEnumerable<DepartmentResponse>>
    {
        private readonly IDepartmentRepository _departmentRepository;

        public GetAllDepartmentHandler(IDepartmentRepository departmentRepository)
        {
            _departmentRepository = departmentRepository;
        }

        public async Task<IEnumerable<DepartmentResponse>> HandleAsync(GetAllDepartmentRequest request, CancellationToken cancellationToken)
        {
            var departments = await _departmentRepository.GetAllAsync();
            return departments.Select(DepartmentResponse.FromEntity);
        }
    }

    public class GetDepartmentByIdRequest : IRequest<DepartmentResponse>
    {
        public required int DepartmentId { get; init; }
    }

    public class GetDepartmentByIdHandler : IRequestHandler<GetDepartmentByIdRequest, DepartmentResponse>
    {
        private readonly IDepartmentRepository _departmentRepository;

        public GetDepartmentByIdHandler(IDepartmentRepository departmentRepository)
        {
            _departmentRepository = departmentRepository;
        }

        public async Task<DepartmentResponse> HandleAsync(GetDepartmentByIdRequest request, CancellationToken cancellationToken)
        {
            var department = await _departmentRepository.GetByIdAsync(request.DepartmentId)
                ?? throw new KeyNotFoundException($"Department {request.DepartmentId} not found.");

            return DepartmentResponse.FromEntity(department);
        }
    }

    public class CreateDepartmentRequest : IRequest<DepartmentResponse>
    {
        public required string Name { get; init; }
    }

    public class CreateDepartmentHandler : IRequestHandler<CreateDepartmentRequest, DepartmentResponse>
    {
        private readonly IDepartmentRepository _departmentRepository;

        public CreateDepartmentHandler(IDepartmentRepository departmentRepository)
        {
            _departmentRepository = departmentRepository;
        }

        public async Task<DepartmentResponse> HandleAsync(CreateDepartmentRequest request, CancellationToken cancellationToken)
        {
            var name = request.Name.Trim();
            var existing = await _departmentRepository.GetByNameAsync(name);
            if (existing is not null)
            {
                throw new InvalidOperationException($"Department '{name}' already exists.");
            }

            var department = new Department { Name = name };
            await _departmentRepository.AddAsync(department);
            await _departmentRepository.SaveChangesAsync();

            return DepartmentResponse.FromEntity(department);
        }
    }

    public class UpdateDepartmentRequest : IRequest<DepartmentResponse>
    {
        public required int DepartmentId { get; init; }
        public required string Name { get; init; }
    }

    public class UpdateDepartmentHandler : IRequestHandler<UpdateDepartmentRequest, DepartmentResponse>
    {
        private readonly IDepartmentRepository _departmentRepository;

        public UpdateDepartmentHandler(IDepartmentRepository departmentRepository)
        {
            _departmentRepository = departmentRepository;
        }

        public async Task<DepartmentResponse> HandleAsync(UpdateDepartmentRequest request, CancellationToken cancellationToken)
        {
            var department = await _departmentRepository.GetByIdAsync(request.DepartmentId)
                ?? throw new KeyNotFoundException($"Department {request.DepartmentId} not found.");

            var name = request.Name.Trim();
            var existing = await _departmentRepository.GetByNameAsync(name);
            if (existing is not null && existing.Id != department.Id)
            {
                throw new InvalidOperationException($"Department '{name}' already exists.");
            }

            department.Name = name;
            _departmentRepository.Update(department);
            await _departmentRepository.SaveChangesAsync();

            return DepartmentResponse.FromEntity(department);
        }
    }

    public class DeleteDepartmentRequest : IRequest<bool>
    {
        public required int DepartmentId { get; init; }
    }

    public class DeleteDepartmentHandler : IRequestHandler<DeleteDepartmentRequest, bool>
    {
        private readonly IDepartmentRepository _departmentRepository;

        public DeleteDepartmentHandler(IDepartmentRepository departmentRepository)
        {
            _departmentRepository = departmentRepository;
        }

        public async Task<bool> HandleAsync(DeleteDepartmentRequest request, CancellationToken cancellationToken)
        {
            var department = await _departmentRepository.GetByIdAsync(request.DepartmentId)
                ?? throw new KeyNotFoundException($"Department {request.DepartmentId} not found.");

            _departmentRepository.Remove(department);
            await _departmentRepository.SaveChangesAsync();

            return true;
        }
    }
}
