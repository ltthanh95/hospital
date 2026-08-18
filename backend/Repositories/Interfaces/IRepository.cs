//This is Interface for respository
//Purpose of this is to create the template/rule for all methods CRUDs => that force everthing must have CRUDs methods for interaction
namespace backend.Repositories.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task AddAsync(T entity);
        void Update(T entity);
        void Remove(T entity);
        Task SaveChangesAsync();
    }
}
