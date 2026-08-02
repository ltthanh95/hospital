using backend.Models;

namespace backend.Repositories.Interfaces
{
    public interface IAppointmentRepository : IRepository<Appointment>
    {
        Task<bool> DoctorHasConflictAsync(int doctorId, DateTime schedule, int? excludeAppointmentId = null);
    }
}
