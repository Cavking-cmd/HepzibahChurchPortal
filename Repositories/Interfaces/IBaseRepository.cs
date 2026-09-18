using System.Linq.Expressions;

namespace ChurchPortal.Repositories.Interfaces
{
    public interface IBaseRepository<T>
    {
        Task CreateAsync(T entity);
        Task Update(T entity);
        Task SoftDeleteAsync(T entity);
        Task DeleteAsync(T entity);
        Task<bool> CheckAsync(Expression<Func<T, bool>> predicate);
        Task<List<T>> GetAllAsync(Expression<Func<T, bool>> predicate);
        Task<T?> GetByIdAsync(Guid id);
    }
}
