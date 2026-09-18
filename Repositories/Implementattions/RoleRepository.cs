using System.Linq.Expressions;
using ChurchPortal.Core.Entities;
using ChurchPortal.DataContext;
using ChurchPortal.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChurchPortal.Repositories.Implementattions
{
    public class RoleRepository : BaseRepository<Role>, IRoleRepository
    {
        private readonly ChurchPortalDbContext _context;
        public RoleRepository(ChurchPortalDbContext context) : base(context)
        {
            _context = context;
        }

        public Task<Role?> GetRoleAsync(Expression<Func<Role, bool>> predicate)
        {
            return _context.Set<Role>().FirstOrDefaultAsync(predicate);
        }

        public Task<Role?> GetRoleByNameAsync(string name)
        {
            return _context.Set<Role>().FirstOrDefaultAsync(a => a.Name == name && !a.IsDeleted);
        }

        public Task<List<Role>> GetAllRolesAsync()
        {
            return _context.Set<Role>().Where(a => !a.IsDeleted).ToListAsync();
        }
    }
}
