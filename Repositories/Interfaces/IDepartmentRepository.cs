using ChurchPortal.Core.Entities;

namespace ChurchPortal.Repositories.Interfaces
{
    public interface IDepartmentRepository : IBaseRepository<Department>
    {
        Task<List<Department>> GetAllOrderedAsync(bool activeOnly);
        Task<Department?> GetByNormalizedNameAsync(string normalizedName);
        Task<Dictionary<Guid, int>> GetRequestCountsAsync();
    }
}
