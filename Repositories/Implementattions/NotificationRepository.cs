using ChurchPortal.Core.Entities;
using ChurchPortal.DataContext;
using ChurchPortal.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChurchPortal.Repositories.Implementattions
{
    public class NotificationRepository : BaseRepository<Notification>, INotificationRepository
    {
        private readonly ChurchPortalDbContext _context;

        public NotificationRepository(ChurchPortalDbContext context) : base(context)
        {
            _context = context;
        }

        public Task<List<Notification>> GetForUserAsync(Guid userId)
        {
            return _context.Notifications
                .AsNoTracking()
                .Where(n => n.UserId == userId && !n.IsDeleted)
                .OrderByDescending(n => n.CreatedDate)
                .Take(50)
                .ToListAsync();
        }

        public async Task<int> MarkAllReadAsync(Guid userId)
        {
            var unread = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead && !n.IsDeleted)
                .ToListAsync();

            foreach (var notification in unread)
            {
                notification.IsRead = true;
            }

            return await _context.SaveChangesAsync();
        }

        public Task<int> UnreadCountAsync(Guid userId)
        {
            return _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead && !n.IsDeleted);
        }
    }
}
