using backend.dbContext;
using backend.Models;
using backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories
{
    public class DepartmentRepository : Repository<Department>, IDepartmentRepository
    {
        public DepartmentRepository(ApplicationDbContext db) : base(db)
        {
        }

        public new async Task<IEnumerable<Department>> GetAllAsync()
        {
            return await _dbSet.Include(department => department.Doctors).ToListAsync();
        }

        public new async Task<Department?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(department => department.Doctors)
                .FirstOrDefaultAsync(department => department.Id == id);
        }

        public async Task<Department?> GetByNameAsync(string name)
        {
            return await _dbSet.FirstOrDefaultAsync(department => department.Name == name);
        }
    }
}
