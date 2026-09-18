using ChurchPortal.Core.Entities;
using ChurchPortal.DataContext;
using ChurchPortal.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChurchPortal.Repositories.Implementattions
{
    public class ServiceRepository : BaseRepository<Service>, IServiceRepository
    {
        private readonly ChurchPortalDbContext _context;
        public ServiceRepository(ChurchPortalDbContext context) : base(context)
        {
            _context = context;
        }

        public Task<List<Service>> GetAllWithAttendancesAsync()
        {
            return _context.Set<Service>()
                .Include(a => a.Attendances)
                .Where(a => !a.IsDeleted)
                .ToListAsync();
        }

        public Task<Service?> GetByIdWithAttendancesAsync(Guid id)
        {
            return _context.Set<Service>()
                .Include(a => a.Attendances)
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);
        }
    }
}
