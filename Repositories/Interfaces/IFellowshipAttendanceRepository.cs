using ChurchPortal.Core.Entities;

namespace ChurchPortal.Repositories.Interfaces
{
    public interface IFellowshipAttendanceRepository : IBaseRepository<FellowshipAttendance>
    {
        Task<List<FellowshipAttendance>> GetAllWithCenterAsync();
        Task<FellowshipAttendance?> GetByIdWithCenterAsync(Guid id);
    }
}
