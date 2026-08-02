using backend.dbContext;
using backend.Models;
using backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories
{
    public class AppointmentRepository : Repository<Appointment>, IAppointmentRepository
    {
        public AppointmentRepository(ApplicationDbContext db) : base(db)
        {
        }

        public new async Task<IEnumerable<Appointment>> GetAllAsync()
        {
            return await _dbSet
                .Include(appointment => appointment.Doctor)
                .Include(appointment => appointment.Patient)
                .Include(appointment => appointment.MedicalRecord)
                .ToListAsync();
        }

        public new async Task<Appointment?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(appointment => appointment.Doctor)
                .Include(appointment => appointment.Patient)
                .Include(appointment => appointment.MedicalRecord)
                .FirstOrDefaultAsync(appointment => appointment.Id == id);
        }

        public async Task<bool> DoctorHasConflictAsync(int doctorId, DateTime schedule, int? excludeAppointmentId = null)
        {
            var date = schedule.Date;
            return await _dbSet.AnyAsync(appointment =>
                appointment.DoctorId == doctorId
                && appointment.Schedule.Date == date
                && appointment.Status != AppointmentStatus.CANCELLED
                && (excludeAppointmentId == null || appointment.Id != excludeAppointmentId));
        }
    }
}
