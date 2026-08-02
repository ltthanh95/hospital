using backend.Models;

namespace backend.Repositories.Interfaces
{
    public interface IDepartmentRepository : IRepository<Department>
    {
        Task<Department?> GetByNameAsync(string name);
    }
}
