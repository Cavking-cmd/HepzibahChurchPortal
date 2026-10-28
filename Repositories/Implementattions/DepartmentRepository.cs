using ChurchPortal.Core.Entities;
using ChurchPortal.DataContext;
using ChurchPortal.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChurchPortal.Repositories.Implementattions
{
    public class DepartmentRepository : BaseRepository<Department>, IDepartmentRepository
    {
        public DepartmentRepository(ChurchPortalDbContext context) : base(context)
        {
        }

        public Task<List<Department>> GetAllOrderedAsync(bool activeOnly)
        {
            return _context.Departments
                .AsNoTracking()
                .Where(d => !d.IsDeleted && (!activeOnly || d.IsActive))
                .OrderBy(d => d.Name)
                .ToListAsync();
        }

        public Task<Department?> GetByNormalizedNameAsync(string normalizedName)
        {
            return _context.Departments.FirstOrDefaultAsync(d => d.NormalizedName == normalizedName && !d.IsDeleted);
        }

        public Task<Dictionary<Guid, int>> GetRequestCountsAsync()
        {
            return _context.ReimbursementRequests
                .AsNoTracking()
                .Where(r => !r.IsDeleted && r.DepartmentId != null)
                .GroupBy(r => r.DepartmentId!.Value)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Count);
        }
    }
}
