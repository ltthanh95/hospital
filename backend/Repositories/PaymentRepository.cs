using backend.dbContext;
using backend.Models;
using backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories
{
    public class PaymentRepository : Repository<Payment>, IPaymentRepository
    {
        public PaymentRepository(ApplicationDbContext db) : base(db)
        {
        }

        public new async Task<IEnumerable<Payment>> GetAllAsync()
        {
            return await _dbSet.Include(payment => payment.Patient).ToListAsync();
        }

        public new async Task<Payment?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(payment => payment.Patient)
                .FirstOrDefaultAsync(payment => payment.Id == id);
        }
    }
}
