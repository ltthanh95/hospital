using backend.dbContext;
using backend.Models;
using backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories
{
    public class StayRepository : Repository<Stay>, IStayRepository
    {
        public StayRepository(ApplicationDbContext db) : base(db)
        {
        }

        public new async Task<IEnumerable<Stay>> GetAllAsync()
        {
            return await _dbSet
                .Include(stay => stay.Patient)
                .Include(stay => stay.Room)
                .ToListAsync();
        }

        public new async Task<Stay?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(stay => stay.Patient)
                .Include(stay => stay.Room)
                .FirstOrDefaultAsync(stay => stay.Id == id);
        }

        public async Task<int> GetActiveOccupancyAsync(int roomId)
        {
            return await _dbSet.CountAsync(stay => stay.RoomId == roomId && stay.CheckOut == null);
        }
    }
}
