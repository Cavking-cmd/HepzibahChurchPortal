using System.Linq.Expressions;
using ChurchPortal.Core.Dtos.Reports;
using ChurchPortal.Core.Entities;

namespace ChurchPortal.Repositories.Interfaces
{
    public interface IReimbursementRepository : IBaseRepository<ReimbursementRequest>
    {
        /// <summary>Tracked, with items, receipts (including file data) and history. For actions on one request.</summary>
        Task<ReimbursementRequest?> GetWithDetailsAsync(Guid id);

        /// <summary>Untracked, with items and history but without receipt files. For lists.</summary>
        Task<List<ReimbursementRequest>> GetAllWithDetailsAsync(Expression<Func<ReimbursementRequest, bool>> predicate);

        /// <summary>Untracked requests with physical items. pendingOnly = only those still waiting for the Inventory Officer.</summary>
        Task<List<ReimbursementRequest>> GetWithPhysicalItemsAsync(bool pendingOnly);

        /// <summary>Tracked request that owns the given line item.</summary>
        Task<ReimbursementRequest?> GetByItemIdAsync(Guid itemId);

        Task<ReimbursementReceipt?> GetReceiptAsync(Guid receiptId);
        Task<Dictionary<Guid, int>> GetReceiptCountsAsync(IReadOnlyCollection<Guid> requestIds);

        /// <summary>Receipt metadata (no file bytes) grouped by request.</summary>
        Task<Dictionary<Guid, List<ReimbursementReceipt>>> GetReceiptMetadataAsync(IReadOnlyCollection<Guid> requestIds);

        /// <summary>Lightweight rows for budget and report calculations. Drafts excluded.</summary>
        Task<List<ReimbursementReportRow>> GetReportRowsAsync(Expression<Func<ReimbursementRequest, bool>>? predicate = null);

        /// <summary>Status transitions for processing-time calculations.</summary>
        Task<List<ReimbursementTransitionRow>> GetTransitionsAsync(IReadOnlyCollection<Guid>? requestIds = null);

        /// <summary>Tracked requests still holding free-text department names.</summary>
        Task<List<ReimbursementRequest>> GetUnlinkedDepartmentRequestsAsync();

        void TrackAsAdded<TEntity>(TEntity entity) where TEntity : class;
        void TrackRangeAsAdded<TEntity>(IEnumerable<TEntity> entities) where TEntity : class;
    }
}
