using System.Linq.Expressions;
using ChurchPortal.Core.Entities;

namespace ChurchPortal.Repositories.Interfaces
{
    public interface IRoleRepository : IBaseRepository<Role>
    {
        Task<Role?> GetRoleAsync(Expression<Func<Role, bool>> predicate);
        Task<Role?> GetRoleByNameAsync(string name);
        Task<List<Role>> GetAllRolesAsync();
    }
}
