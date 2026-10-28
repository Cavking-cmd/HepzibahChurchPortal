using ChurchPortal.Core.Entities;
using ChurchPortal.DataContext;
using Microsoft.EntityFrameworkCore;

namespace ChurchPortal.Services.Interfaces
{
    /// <summary>
    /// Runs at startup, like the other seeders. Safe to run every time.
    ///
    /// The old workflow had a minister-approval stage (status 2, "AccountsReview" = approved by
    /// the minister and waiting for the Accounts Officer). That stage is gone, so any request still
    /// sitting there is moved to Under Review, where an Accounts Officer picks it up. A history entry
    /// records the move. Nothing else is changed: approvals and payments made under the old process
    /// are kept exactly as they were, and no approvals are invented.
    /// </summary>
    public static class ReimbursementLegacyFixer
    {
        public static async Task RunAsync(ChurchPortalDbContext context, ILogger logger)
        {
            var stuck = await context.ReimbursementRequests
                .Where(r => !r.IsDeleted && r.Status == ReimbursementStatus.LegacyAccountsReview)
                .ToListAsync();

            if (stuck.Count == 0)
            {
                return;
            }

            foreach (var request in stuck)
            {
                request.Status = ReimbursementStatus.UnderReview;
                context.ReimbursementHistories.Add(new ReimbursementHistory
                {
                    ReimbursementRequestId = request.Id,
                    ActorUserId = Guid.Empty,
                    ActorName = "System",
                    Action = "WorkflowMigrated",
                    FromStatus = ReimbursementStatus.LegacyAccountsReview,
                    ToStatus = ReimbursementStatus.UnderReview,
                    Notes = "The minister-approval stage was removed from the process. This request moved to Under Review for an Accounts Officer.",
                    Timestamp = DateTime.UtcNow
                });
            }

            await context.SaveChangesAsync();
            logger.LogInformation("Moved {Count} reimbursement(s) from the retired minister-approval stage to Under Review.", stuck.Count);
        }
    }
}
