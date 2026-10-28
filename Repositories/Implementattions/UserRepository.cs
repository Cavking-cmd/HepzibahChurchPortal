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
            // Case-insensitive so "John@x.com" and "john@x.com" sign in to the same account.
            var normalized = (email ?? string.Empty).Trim().ToLower();
            return _context.Set<User>()
                .Include(a => a.UserRoles)
                .ThenInclude(a => a.Role)
                .FirstOrDefaultAsync(a => a.Email.ToLower() == normalized && !a.IsDeleted);
        }

        public Task<bool> EmailExistsAsync(string email)
        {
            var normalized = (email ?? string.Empty).Trim().ToLower();
            return _context.Set<User>().AnyAsync(a => a.Email.ToLower() == normalized && !a.IsDeleted);
        }

        public async Task<Dictionary<Guid, string>> GetDisplayNamesAsync(IEnumerable<Guid> userIds)
        {
            var ids = userIds.Where(id => id != Guid.Empty).Distinct().ToList();
            if (ids.Count == 0)
            {
                return new Dictionary<Guid, string>();
            }

            var rows = await _context.Set<User>()
                .AsNoTracking()
                .Where(u => ids.Contains(u.Id))
                .Select(u => new { u.Id, u.DisplayName, u.Email })
                .ToListAsync();

            return rows.ToDictionary(u => u.Id, u => string.IsNullOrWhiteSpace(u.DisplayName) ? u.Email : u.DisplayName!);
        }

        public Task<User?> GetUserAsync(Expression<Func<User, bool>> predicate)
        {
            return _context.Set<User>()
                .Include(a => a.UserRoles)
                .ThenInclude(a => a.Role)
                .FirstOrDefaultAsync(predicate);
        }

        public Task<User?> GetByPasswordResetTokenAsync(string token)
        {
            return _context.Set<User>()
                .Include(a => a.UserRoles)
                .ThenInclude(a => a.Role)
                .FirstOrDefaultAsync(a => a.PasswordResetToken == token && !a.IsDeleted);
        }

        public Task<User?> GetByEmailVerificationTokenAsync(string token)
        {
            return _context.Set<User>()
                .Include(a => a.UserRoles)
                .ThenInclude(a => a.Role)
                .FirstOrDefaultAsync(a => a.EmailVerificationToken == token && !a.IsDeleted);
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

        public Task<List<User>> GetUsersByRoleAsync(string roleName)
        {
            return _context.Set<User>()
                .Include(a => a.UserRoles)
                .ThenInclude(a => a.Role)
                .Where(a => !a.IsDeleted &&
                    a.UserRoles.Any(ur => !ur.IsDeleted && ur.Role != null && ur.Role.Name == roleName && !ur.Role.IsDeleted))
                .OrderBy(a => a.Email)
                .ToListAsync();
        }
    }
}
