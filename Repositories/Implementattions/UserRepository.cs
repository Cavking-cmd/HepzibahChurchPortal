using System.Linq.Expressions;
using ChurchPortal.Core.Entities;
using ChurchPortal.DataContext;
using ChurchPortal.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChurchPortal.Repositories.Implementattions
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        private readonly ChurchPortalDbContext _context;
        public UserRepository(ChurchPortalDbContext context) : base(context)
        {
            _context = context;
        }

        public override async Task<User?> GetByIdAsync(Guid id)
        {
            return await _context.Set<User>()
                .Include(a => a.UserRoles)
                .ThenInclude(a => a.Role)
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);
        }

        public Task<User?> GetUserByEmailAsync(string email)
        {
            return _context.Set<User>()
                .Include(a => a.UserRoles)
                .ThenInclude(a => a.Role)
                .FirstOrDefaultAsync(a => a.Email == email && !a.IsDeleted);
        }

        public Task<User?> GetUserAsync(Expression<Func<User, bool>> predicate)
        {
            return _context.Set<User>()
                .Include(a => a.UserRoles)
                .ThenInclude(a => a.Role)
                .FirstOrDefaultAsync(predicate);
        }

        public Task<List<User>> GetAllUsersAsync()
        {
            return _context.Set<User>()
                .Include(a => a.UserRoles)
                .ThenInclude(a => a.Role)
                .Where(a => !a.IsDeleted)
                .OrderBy(a => a.Email)
                .ToListAsync();
        }
    }
}
