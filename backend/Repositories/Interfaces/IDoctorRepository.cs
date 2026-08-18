//Template for Doctor with the all methods from IRepository
using backend.Models;

namespace backend.Repositories.Interfaces
{
    public interface IDoctorRepository : IRepository<Doctor>
    {
        Task<Doctor?> GetByUserIdAsync(int userId);
    }
}
