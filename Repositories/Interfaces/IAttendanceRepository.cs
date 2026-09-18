using ChurchPortal.Core.Entities;

namespace ChurchPortal.Repositories.Interfaces
{
    public interface IAttendanceRepository : IBaseRepository<Attendance>
    {
        Task<List<Attendance>> GetAllWithServiceAsync();
        Task<Attendance?> GetByIdWithServiceAsync(Guid id);
    }
}
