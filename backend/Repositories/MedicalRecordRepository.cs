using backend.dbContext;
using backend.Models;
using backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories
{
    public class MedicalRecordRepository : Repository<MedicalRecord>, IMedicalRecordRepository
    {
        public MedicalRecordRepository(ApplicationDbContext db) : base(db)
        {
        }

        public new async Task<IEnumerable<MedicalRecord>> GetAllAsync()
        {
            return await _dbSet
                .Include(record => record.Doctor)
                .Include(record => record.Patient)
                .Include(record => record.Prescriptions).ThenInclude(prescription => prescription.PrescriptionItems)
                .ToListAsync();
        }

        public new async Task<MedicalRecord?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(record => record.Doctor)
                .Include(record => record.Patient)
                .Include(record => record.Prescriptions).ThenInclude(prescription => prescription.PrescriptionItems)
                .FirstOrDefaultAsync(record => record.Id == id);
        }
    }
}
