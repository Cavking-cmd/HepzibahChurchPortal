using ChurchPortal.Core.Entities;
using ChurchPortal.DataContext;
using ChurchPortal.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChurchPortal.Repositories.Implementattions
{
    public class FellowshipAttendanceRepository : BaseRepository<FellowshipAttendance>, IFellowshipAttendanceRepository
    {
        private readonly ChurchPortalDbContext _context;
        public FellowshipAttendanceRepository(ChurchPortalDbContext context) : base(context)
        {
            _context = context;
        }

        public override async Task<FellowshipAttendance?> GetByIdAsync(Guid id)
        {
            return await _context.Set<FellowshipAttendance>()
                .Include(a => a.FellowshipCenter)
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);
        }

        public Task<List<FellowshipAttendance>> GetAllWithCenterAsync()
        {
            return _context.Set<FellowshipAttendance>()
                .Include(a => a.FellowshipCenter)
                .Where(a => !a.IsDeleted)
                .ToListAsync();
        }

        public Task<FellowshipAttendance?> GetByIdWithCenterAsync(Guid id)
        {
            return _context.Set<FellowshipAttendance>()
                .Include(a => a.FellowshipCenter)
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);
        }
    }
}
