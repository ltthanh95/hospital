using backend.dbContext;
using backend.Models;
using backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories
{
    public class RoomRepository : Repository<Room>, IRoomRepository
    {
        public RoomRepository(ApplicationDbContext db) : base(db)
        {
        }

        public new async Task<IEnumerable<Room>> GetAllAsync()
        {
            return await _dbSet.Include(room => room.Stays).ToListAsync();
        }

        public new async Task<Room?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(room => room.Stays)
                .FirstOrDefaultAsync(room => room.Id == id);
        }
    }
}
