using System.Linq.Expressions;
using ChurchPortal.Core.Entities;

namespace ChurchPortal.Repositories.Interfaces
{
    public interface IReimbursementRepository : IBaseRepository<ReimbursementRequest>
    {
        Task<ReimbursementRequest?> GetWithDetailsAsync(Guid id);
        Task<List<ReimbursementRequest>> GetAllWithDetailsAsync(Expression<Func<ReimbursementRequest, bool>> predicate);
        Task<ReimbursementReceipt?> GetReceiptAsync(Guid receiptId);
        Task<Dictionary<Guid, int>> GetReceiptCountsAsync(IReadOnlyCollection<Guid> requestIds);
        void TrackAsAdded<TEntity>(TEntity entity) where TEntity : class;
        void TrackRangeAsAdded<TEntity>(IEnumerable<TEntity> entities) where TEntity : class;
    }
}
