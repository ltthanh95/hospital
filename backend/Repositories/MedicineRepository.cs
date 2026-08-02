using backend.dbContext;
using backend.Models;
using backend.Repositories.Interfaces;

namespace backend.Repositories
{
    public class MedicineRepository : Repository<Medicine>, IMedicineRepository
    {
        public MedicineRepository(ApplicationDbContext db) : base(db)
        {
        }
    }
}
