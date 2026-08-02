using backend.dbContext;
using backend.Models;
using backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories
{
    public class DoctorRepository : Repository<Doctor>, IDoctorRepository
    {
        public DoctorRepository(ApplicationDbContext db) : base(db)
        {
        }

        public new async Task<IEnumerable<Doctor>> GetAllAsync()
        {
            return await _dbSet
                .Include(doctor => doctor.User)
                .Include(doctor => doctor.Department)
                .Include(doctor => doctor.Appointment).ThenInclude(appointment => appointment.Patient)
                .Include(doctor => doctor.MedicalRecords).ThenInclude(record => record.Patient)
                .ToListAsync();
        }

        public new async Task<Doctor?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(doctor => doctor.User)
                .Include(doctor => doctor.Department)
                .Include(doctor => doctor.Appointment).ThenInclude(appointment => appointment.Patient)
                .Include(doctor => doctor.MedicalRecords).ThenInclude(record => record.Patient)
                .FirstOrDefaultAsync(doctor => doctor.Id == id);
        }

        public async Task<Doctor?> GetByUserIdAsync(int userId)
        {
            return await _dbSet.FirstOrDefaultAsync(doctor => doctor.UserId == userId);
        }
    }
}
