using ChurchPortal.Core.Entities;
using ChurchPortal.DataContext;
using ChurchPortal.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChurchPortal.Repositories.Implementattions
{
    public class PastorRepository : BaseRepository<Pastor>, IPastorRepository
    {
        public PastorRepository(ChurchPortalDbContext context) : base(context)
        {
        }

        public Task<List<Pastor>> GetAllOrderedAsync(bool activeOnly)
        {
            return _context.Pastors
                .AsNoTracking()
                .Where(p => !p.IsDeleted && (!activeOnly || p.IsActive))
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public Task<Dictionary<Guid, int>> GetServiceCountsAsync()
        {
            return _context.Services
                .AsNoTracking()
                .Where(s => !s.IsDeleted && s.PastorId != null)
                .GroupBy(s => s.PastorId!.Value)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Count);
        }

        public Task<List<Service>> GetUnlinkedServicesWithPreacherAsync()
        {
            return _context.Services
                .Where(s => !s.IsDeleted && s.PastorId == null && s.Preacher != null && s.Preacher != "")
                .ToListAsync();
        }

        public Task<List<Service>> GetServicesForPastorAsync(Guid pastorId)
        {
            return _context.Services
                .Where(s => !s.IsDeleted && s.PastorId == pastorId)
                .ToListAsync();
        }
    }
}
