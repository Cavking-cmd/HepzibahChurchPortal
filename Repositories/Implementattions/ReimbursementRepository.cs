using System.Linq.Expressions;
using ChurchPortal.Core.Dtos.Reports;
using ChurchPortal.Core.Entities;
using ChurchPortal.DataContext;
using ChurchPortal.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChurchPortal.Repositories.Implementattions
{
    public class ReimbursementRepository : BaseRepository<ReimbursementRequest>, IReimbursementRepository
    {
        public ReimbursementRepository(ChurchPortalDbContext context) : base(context)
        {
        }

        public Task<ReimbursementRequest?> GetWithDetailsAsync(Guid id)
        {
            return _context.ReimbursementRequests
                .Include(r => r.Items)
                .Include(r => r.Receipts)
                .Include(r => r.History)
                .Include(r => r.DepartmentRecord)
                .AsSplitQuery()
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
        }

        public Task<List<ReimbursementRequest>> GetAllWithDetailsAsync(Expression<Func<ReimbursementRequest, bool>> predicate)
        {
            return _context.ReimbursementRequests
                .AsNoTracking()
                .Include(r => r.Items)
                .Include(r => r.History)
                .Include(r => r.DepartmentRecord)
                .AsSplitQuery()
                .Where(predicate)
                .Where(r => !r.IsDeleted)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();
        }

        /// <summary>Requests that have at least one physical item, for the Inventory Officer.</summary>
        public Task<List<ReimbursementRequest>> GetWithPhysicalItemsAsync(bool pendingOnly)
        {
            var query = _context.ReimbursementRequests
                .AsNoTracking()
                .Include(r => r.Items)
                .Include(r => r.DepartmentRecord)
                .AsSplitQuery()
                .Where(r => !r.IsDeleted && r.Status != ReimbursementStatus.Draft);

            query = pendingOnly
                ? query.Where(r => r.Items.Any(i => !i.IsDeleted && i.VerificationStatus == InventoryVerificationStatus.Pending))
                : query.Where(r => r.Items.Any(i => !i.IsDeleted && i.VerificationStatus != InventoryVerificationStatus.NotRequired));

            return query.OrderBy(r => r.SubmittedDate).ToListAsync();
        }

        /// <summary>The request that owns a line item, tracked, with everything needed to change it.</summary>
        public async Task<ReimbursementRequest?> GetByItemIdAsync(Guid itemId)
        {
            var requestId = await _context.ReimbursementItems
                .Where(i => i.Id == itemId && !i.IsDeleted)
                .Select(i => (Guid?)i.ReimbursementRequestId)
                .FirstOrDefaultAsync();

            return requestId == null ? null : await GetWithDetailsAsync(requestId.Value);
        }

        public Task<ReimbursementReceipt?> GetReceiptAsync(Guid receiptId)
        {
            return _context.ReimbursementReceipts
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == receiptId && !r.IsDeleted);
        }

        public void TrackAsAdded<TEntity>(TEntity entity) where TEntity : class
        {
            _context.Set<TEntity>().Add(entity);
        }

        public void TrackRangeAsAdded<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
        {
            _context.Set<TEntity>().AddRange(entities);
        }

        public async Task<Dictionary<Guid, int>> GetReceiptCountsAsync(IReadOnlyCollection<Guid> requestIds)
        {
            if (requestIds.Count == 0)
            {
                return new Dictionary<Guid, int>();
            }

            return await _context.ReimbursementReceipts
                .AsNoTracking()
                .Where(r => requestIds.Contains(r.ReimbursementRequestId) && !r.IsDeleted)
                .GroupBy(r => r.ReimbursementRequestId)
                .Select(g => new { RequestId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.RequestId, x => x.Count);
        }

        public async Task<Dictionary<Guid, List<ReimbursementReceipt>>> GetReceiptMetadataAsync(IReadOnlyCollection<Guid> requestIds)
        {
            if (requestIds.Count == 0)
            {
                return new Dictionary<Guid, List<ReimbursementReceipt>>();
            }

            var rows = await _context.ReimbursementReceipts
                .AsNoTracking()
                .Where(r => requestIds.Contains(r.ReimbursementRequestId) && !r.IsDeleted)
                .Select(r => new
                {
                    r.Id,
                    r.ReimbursementRequestId,
                    r.FileName,
                    r.ContentType,
                    r.FileSize,
                    r.UploadedBy,
                    r.CreatedDate
                })
                .ToListAsync();

            return rows
                .GroupBy(r => r.ReimbursementRequestId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(r => new ReimbursementReceipt
                    {
                        Id = r.Id,
                        ReimbursementRequestId = r.ReimbursementRequestId,
                        FileName = r.FileName,
                        ContentType = r.ContentType,
                        FileSize = r.FileSize,
                        FileData = [],
                        UploadedBy = r.UploadedBy,
                        CreatedDate = r.CreatedDate
                    }).ToList());
        }

        public Task<List<ReimbursementReportRow>> GetReportRowsAsync(Expression<Func<ReimbursementRequest, bool>>? predicate = null)
        {
            var query = _context.ReimbursementRequests
                .AsNoTracking()
                .Where(r => !r.IsDeleted && r.Status != ReimbursementStatus.Draft);

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            return query
                .Select(r => new ReimbursementReportRow
                {
                    Id = r.Id,
                    RequestNumber = r.RequestNumber,
                    PayeeUserId = r.PayeeUserId,
                    PayeeName = r.PayeeName,
                    Classification = r.Classification,
                    DepartmentId = r.DepartmentId,
                    DepartmentText = r.Department,
                    DepartmentName = r.DepartmentRecord != null ? r.DepartmentRecord.Name : null,
                    Status = r.Status,
                    TotalAmount = r.TotalAmount,
                    ApprovedAmount = r.ApprovedAmount,
                    AmountPaid = r.AmountPaid,
                    RequestDate = r.RequestDate,
                    CreatedDate = r.CreatedDate,
                    SubmittedDate = r.SubmittedDate,
                    ReviewedDate = r.ReviewedDate,
                    ApprovedDate = r.ApprovedDate,
                    PaymentDate = r.PaymentDate,
                    RejectedDate = r.RejectedDate
                })
                .ToListAsync();
        }

        public Task<List<ReimbursementTransitionRow>> GetTransitionsAsync(IReadOnlyCollection<Guid>? requestIds = null)
        {
            var query = _context.ReimbursementHistories
                .AsNoTracking()
                .Where(h => !h.IsDeleted && h.ToStatus != null);

            if (requestIds != null)
            {
                query = query.Where(h => requestIds.Contains(h.ReimbursementRequestId));
            }

            return query
                .OrderBy(h => h.Timestamp)
                .Select(h => new ReimbursementTransitionRow
                {
                    RequestId = h.ReimbursementRequestId,
                    FromStatus = h.FromStatus,
                    ToStatus = h.ToStatus!.Value,
                    Timestamp = h.Timestamp
                })
                .ToListAsync();
        }

        public Task<List<ReimbursementRequest>> GetUnlinkedDepartmentRequestsAsync()
        {
            return _context.ReimbursementRequests
                .Where(r => !r.IsDeleted
                    && r.DepartmentId == null
                    && r.Classification == ReimbursementClassification.Department)
                .ToListAsync();
        }
    }
}
