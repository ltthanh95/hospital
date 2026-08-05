using backend.dbContext;
using backend.Models;
using backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories
{
    public class PatientRepository : Repository<Patient>, IPatientRepository
    {
        public PatientRepository(ApplicationDbContext db) : base(db)
        {
        }

        public new async Task<IEnumerable<Patient>> GetAllAsync()
        {
            return await _dbSet
                .Include(patient => patient.User)
                .Include(patient => patient.Appointment).ThenInclude(appointment => appointment.Doctor)
                .Include(patient => patient.MedicalRecords).ThenInclude(record => record.Doctor)
                .ToListAsync();
        }

        public new async Task<Patient?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(patient => patient.User)
                .Include(patient => patient.Appointment).ThenInclude(appointment => appointment.Doctor)
                .Include(patient => patient.MedicalRecords).ThenInclude(record => record.Doctor)
                .FirstOrDefaultAsync(patient => patient.Id == id);
        }

        public async Task<Patient?> GetByUserIdAsync(int userId)
        {
            return await _dbSet
                .Include(patient => patient.User)
                .Include(patient => patient.Appointment).ThenInclude(appointment => appointment.Doctor)
                .Include(patient => patient.MedicalRecords).ThenInclude(record => record.Doctor)
                .FirstOrDefaultAsync(patient => patient.UserId == userId);
        }
    }
}
