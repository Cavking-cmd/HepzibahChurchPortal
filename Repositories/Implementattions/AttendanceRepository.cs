using ChurchPortal.Core.Entities;
using ChurchPortal.DataContext;
using ChurchPortal.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChurchPortal.Repositories.Implementattions
{
    public class AttendanceRepository : BaseRepository<Attendance>, IAttendanceRepository
    {
        private readonly ChurchPortalDbContext _context;
        public AttendanceRepository(ChurchPortalDbContext context) : base(context)
        {
            _context = context;
        }

        public override async Task<Attendance?> GetByIdAsync(Guid id)
        {
            return await _context.Set<Attendance>()
                .Include(a => a.Service)
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);
        }

        public Task<List<Attendance>> GetAllWithServiceAsync()
        {
            return _context.Set<Attendance>()
                .Include(a => a.Service)
                .Where(a => !a.IsDeleted)
                .ToListAsync();
        }

        public Task<Attendance?> GetByIdWithServiceAsync(Guid id)
        {
            return _context.Set<Attendance>()
                .Include(a => a.Service)
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);
        }
    }
}
