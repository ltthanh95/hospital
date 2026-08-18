using backend.Models;

//Template for Appointment with the all methods from IRepository
namespace backend.Repositories.Interfaces
{
    public interface IAppointmentRepository : IRepository<Appointment>
    {
        Task<bool> DoctorHasConflictAsync(int doctorId, DateTime schedule, int? excludeAppointmentId = null);
    }
}
