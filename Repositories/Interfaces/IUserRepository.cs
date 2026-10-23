using System.Linq.Expressions;
using ChurchPortal.Core.Entities;

namespace ChurchPortal.Repositories.Interfaces
{
    public interface IUserRepository : IBaseRepository<User>
    {
        Task<User?> GetUserAsync(Expression<Func<User, bool>> predicate);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetByPasswordResetTokenAsync(string token);
        Task<User?> GetByEmailVerificationTokenAsync(string token);
        Task<List<User>> GetAllUsersAsync();
        Task<List<User>> GetUsersByRoleAsync(string roleName);
    }
}
