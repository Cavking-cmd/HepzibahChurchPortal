using ChurchPortal.Core.Entities;

namespace ChurchPortal.Repositories.Interfaces
{
    public interface IServiceRepository : IBaseRepository<Service>
    {
        Task<List<Service>> GetAllWithAttendancesAsync();
        Task<Service?> GetByIdWithAttendancesAsync(Guid id);
    }
}
