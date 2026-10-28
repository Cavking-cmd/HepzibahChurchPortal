using ChurchPortal.Core.Entities;

namespace ChurchPortal.Repositories.Interfaces
{
    public interface IPastorRepository : IBaseRepository<Pastor>
    {
        Task<List<Pastor>> GetAllOrderedAsync(bool activeOnly);
        Task<Dictionary<Guid, int>> GetServiceCountsAsync();
        Task<List<Service>> GetUnlinkedServicesWithPreacherAsync();
        Task<List<Service>> GetServicesForPastorAsync(Guid pastorId);
    }
}
