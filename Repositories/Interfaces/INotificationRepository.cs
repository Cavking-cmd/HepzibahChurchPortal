using ChurchPortal.Core.Entities;

namespace ChurchPortal.Repositories.Interfaces
{
    public interface INotificationRepository : IBaseRepository<Notification>
    {
        Task<List<Notification>> GetForUserAsync(Guid userId);
        Task<int> MarkAllReadAsync(Guid userId);
        Task<int> UnreadCountAsync(Guid userId);
    }
}
