using backend.Models;

namespace backend.Repositories.Interfaces
{
    public interface IStayRepository : IRepository<Stay>
    {
        Task<int> GetActiveOccupancyAsync(int roomId);
    }
}
