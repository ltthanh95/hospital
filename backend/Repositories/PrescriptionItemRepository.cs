using backend.dbContext;
using backend.Models;
using backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories
{
    public class PrescriptionItemRepository : Repository<PrescriptionItem>, IPrescriptionItemRepository
    {
        public PrescriptionItemRepository(ApplicationDbContext db) : base(db)
        {
        }

        public new async Task<IEnumerable<PrescriptionItem>> GetAllAsync()
        {
            return await _dbSet.Include(item => item.Medicine).ToListAsync();
        }

        public new async Task<PrescriptionItem?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(item => item.Medicine)
                .FirstOrDefaultAsync(item => item.Id == id);
        }
    }
}
