//Template for Dept with the all methods from IRepository
using backend.Models;


namespace backend.Repositories.Interfaces
{
    public interface IDepartmentRepository : IRepository<Department>
    {
        Task<Department?> GetByNameAsync(string name);
    }
}
