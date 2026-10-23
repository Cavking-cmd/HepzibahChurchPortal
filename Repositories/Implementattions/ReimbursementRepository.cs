using System.Linq.Expressions;
using ChurchPortal.Core.Entities;
using ChurchPortal.DataContext;
using ChurchPortal.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChurchPortal.Repositories.Implementattions
{
    public class ReimbursementRepository : BaseRepository<ReimbursementRequest>, IReimbursementRepository
    {
        private readonly ChurchPortalDbContext _context;

        public ReimbursementRepository(ChurchPortalDbContext context) : base(context)
        {
            _context = context;
        }

        public Task<ReimbursementRequest?> GetWithDetailsAsync(Guid id)
        {
            return _context.ReimbursementRequests
                .Include(r => r.Items)
                .Include(r => r.Receipts)
                .Include(r => r.History)
                .AsSplitQuery()
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
        }

        public Task<List<ReimbursementRequest>> GetAllWithDetailsAsync(Expression<Func<ReimbursementRequest, bool>> predicate)
        {
            return _context.ReimbursementRequests
                .AsNoTracking()
                .Include(r => r.Items)
                .Include(r => r.History)
                .AsSplitQuery()
                .Where(predicate)
                .Where(r => !r.IsDeleted)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();
        }

        public Task<ReimbursementReceipt?> GetReceiptAsync(Guid receiptId)
        {
            return _context.ReimbursementReceipts
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == receiptId && !r.IsDeleted);
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
    }
}
