using backend.dbContext;
using backend.Models;
using backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories
{
    public class InvoiceRepository : Repository<Invoice>, IInvoiceRepository
    {
        public InvoiceRepository(ApplicationDbContext db) : base(db)
        {
        }

        public new async Task<IEnumerable<Invoice>> GetAllAsync()
        {
            return await _dbSet
                .Include(invoice => invoice.Patient)
                .Include(invoice => invoice.InvoiceItems)
                .ToListAsync();
        }

        public new async Task<Invoice?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(invoice => invoice.Patient)
                .Include(invoice => invoice.InvoiceItems)
                .FirstOrDefaultAsync(invoice => invoice.Id == id);
        }
    }
}
