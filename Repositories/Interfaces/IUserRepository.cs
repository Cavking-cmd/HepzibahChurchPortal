using System.Linq.Expressions;
using ChurchPortal.Core.Entities;

namespace ChurchPortal.Repositories.Interfaces
{
    public interface IUserRepository : IBaseRepository<User>
    {
        Task<User?> GetUserAsync(Expression<Func<User, bool>> predicate);
        Task<User?> GetUserByEmailAsync(string email);
    }
}
