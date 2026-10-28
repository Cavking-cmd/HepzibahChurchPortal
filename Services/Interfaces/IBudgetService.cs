using ChurchPortal.Core.Dtos.Reports;
using ChurchPortal.Core.Dtos.Request;
using ChurchPortal.Core.Entities;

namespace ChurchPortal.Services.Interfaces
{
    public class BudgetOptions
    {
        /// <summary>Warn when (paid + committed) reaches this share of the annual budget.</summary>
        public decimal WarningThresholdPercent { get; set; } = 80m;

        /// <summary>Flag when pending requests are at least this share of what is left.</summary>
        public decimal HighPendingSharePercent { get; set; } = 50m;
    }

    public interface IBudgetService
    {
        decimal WarningThresholdPercent { get; }
        decimal HighPendingSharePercent { get; }

        /// <summary>Pure calculation over already-loaded rows; used by reports so every figure matches.</summary>
        DepartmentBudgetDto Calculate(Department department, IEnumerable<ReimbursementReportRow> rows, int year, int month);

        Task<List<DepartmentBudgetDto>> GetAllDepartmentBudgetsAsync(int year, int month, bool includeInactive);
        Task<DepartmentBudgetDto?> GetDepartmentBudgetAsync(Guid departmentId, int year, int month);

        /// <summary>
        /// Budget position for a request, projected as if it were approved now with
        /// <paramref name="amountOverride"/> (or its approved amount, else its total).
        /// </summary>
        Task<ReimbursementBudgetDto> GetRequestBudgetAsync(ReimbursementRequest request, decimal? amountOverride = null);

        // Shared date rules so budgets and reports agree.
        DateTime PaidOn(ReimbursementReportRow row);
        DateTime CommittedOn(ReimbursementReportRow row);
        DateTime SubmittedOn(ReimbursementReportRow row);
        bool IsPending(ReimbursementStatus status);
    }
}
