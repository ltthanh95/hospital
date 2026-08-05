using backend.dbContext;
using backend.Models;
using backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories
{
    public class PrescriptionRepository : Repository<Prescription>, IPrescriptionRepository
    {
        public PrescriptionRepository(ApplicationDbContext db) : base(db)
        {
        }

        public new async Task<IEnumerable<Prescription>> GetAllAsync()
        {
            return await _dbSet
                .Include(prescription => prescription.PrescriptionItems).ThenInclude(item => item.Medicine)
                .ToListAsync();
        }

        public new async Task<Prescription?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(prescription => prescription.PrescriptionItems).ThenInclude(item => item.Medicine)
                .FirstOrDefaultAsync(prescription => prescription.Id == id);
        }
    }
}
