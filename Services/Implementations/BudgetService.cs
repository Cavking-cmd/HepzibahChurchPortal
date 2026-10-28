using System.Globalization;
using ChurchPortal.Core.Dtos.Reports;
using ChurchPortal.Core.Dtos.Request;
using ChurchPortal.Core.Entities;
using ChurchPortal.Repositories.Interfaces;
using ChurchPortal.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace ChurchPortal.Services.Implementations
{
    /// <summary>
    /// The single place department budget figures are calculated.
    ///
    /// Buckets (a request is in exactly one at a time, so nothing is double counted):
    ///   Paid      - status Paid. Amount = amount paid. Period = payment date.
    ///   Committed - status Approved (not paid yet). Amount = approved amount, else total. Period = approval date.
    ///   Pending   - status Under Review or Pending Approval. Amount = requested total. Period = submission date.
    ///               Shown separately; never treated as spent.
    /// Remaining   = budget - paid - committed.
    /// Utilisation = (paid + committed) / budget.
    /// Rejected, returned-for-correction and draft requests are not counted anywhere.
    /// Personal Use requests never touch a department budget.
    /// </summary>
    public class BudgetService : IBudgetService
    {
        private static readonly CultureInfo Money = CultureInfo.GetCultureInfo("en-CA");

        private readonly IDepartmentRepository _departmentRepository;
        private readonly IReimbursementRepository _reimbursementRepository;
        private readonly BudgetOptions _options;

        public BudgetService(
            IDepartmentRepository departmentRepository,
            IReimbursementRepository reimbursementRepository,
            IOptions<BudgetOptions> options)
        {
            _departmentRepository = departmentRepository;
            _reimbursementRepository = reimbursementRepository;
            _options = options.Value;
        }

        public decimal WarningThresholdPercent => _options.WarningThresholdPercent;
        public decimal HighPendingSharePercent => _options.HighPendingSharePercent;

        public DateTime PaidOn(ReimbursementReportRow row) =>
            row.PaymentDate ?? row.ApprovedDate ?? row.SubmittedDate ?? row.CreatedDate;

        public DateTime CommittedOn(ReimbursementReportRow row) =>
            row.ApprovedDate ?? row.SubmittedDate ?? row.CreatedDate;

        public DateTime SubmittedOn(ReimbursementReportRow row) =>
            row.SubmittedDate ?? row.CreatedDate;

        public bool IsPending(ReimbursementStatus status) =>
            status is ReimbursementStatus.UnderReview
                or ReimbursementStatus.PendingApproval
                or ReimbursementStatus.LegacyAccountsReview;

        public DepartmentBudgetDto Calculate(Department department, IEnumerable<ReimbursementReportRow> rows, int year, int month)
        {
            var mine = rows
                .Where(r => r.DepartmentId == department.Id && r.Classification == ReimbursementClassification.Department)
                .ToList();

            decimal Sum(Func<ReimbursementReportRow, bool> filter, Func<ReimbursementReportRow, decimal> amount) =>
                mine.Where(filter).Sum(amount);

            var paidYear = Sum(r => r.Status == ReimbursementStatus.Paid && PaidOn(r).Year == year, r => r.Paid);
            var committedYear = Sum(r => r.Status == ReimbursementStatus.Approved && CommittedOn(r).Year == year, r => r.Committed);
            var pendingYear = Sum(r => IsPending(r.Status) && SubmittedOn(r).Year == year, r => r.Requested);

            var paidMonth = Sum(r => r.Status == ReimbursementStatus.Paid && InMonth(PaidOn(r), year, month), r => r.Paid);
            var committedMonth = Sum(r => r.Status == ReimbursementStatus.Approved && InMonth(CommittedOn(r), year, month), r => r.Committed);
            var pendingMonth = Sum(r => IsPending(r.Status) && InMonth(SubmittedOn(r), year, month), r => r.Requested);

            var dto = new DepartmentBudgetDto
            {
                DepartmentId = department.Id,
                DepartmentName = department.Name,
                IsActive = department.IsActive,
                Year = year,
                Month = month,
                AnnualBudget = department.AnnualBudget,
                PaidThisYear = paidYear,
                CommittedThisYear = committedYear,
                PendingThisYear = pendingYear,
                RemainingThisYear = department.AnnualBudget - paidYear - committedYear,
                UtilisationPercentThisYear = Percent(paidYear + committedYear, department.AnnualBudget),
                MonthlyBudget = department.MonthlyBudget,
                PaidThisMonth = paidMonth,
                CommittedThisMonth = committedMonth,
                PendingThisMonth = pendingMonth,
                RemainingThisMonth = department.MonthlyBudget.HasValue
                    ? department.MonthlyBudget.Value - paidMonth - committedMonth
                    : null,
                UtilisationPercentThisMonth = department.MonthlyBudget.HasValue
                    ? Percent(paidMonth + committedMonth, department.MonthlyBudget.Value)
                    : null
            };

            dto.Alerts = BuildDepartmentAlerts(dto);
            return dto;
        }

        public async Task<List<DepartmentBudgetDto>> GetAllDepartmentBudgetsAsync(int year, int month, bool includeInactive)
        {
            var departments = await _departmentRepository.GetAllOrderedAsync(activeOnly: !includeInactive);
            var rows = await _reimbursementRepository.GetReportRowsAsync(r => r.DepartmentId != null);
            return departments.Select(d => Calculate(d, rows, year, month)).ToList();
        }

        public async Task<DepartmentBudgetDto?> GetDepartmentBudgetAsync(Guid departmentId, int year, int month)
        {
            var department = await _departmentRepository.GetByIdAsync(departmentId);
            if (department == null)
            {
                return null;
            }

            var rows = await _reimbursementRepository.GetReportRowsAsync(r => r.DepartmentId == departmentId);
            return Calculate(department, rows, year, month);
        }

        public async Task<ReimbursementBudgetDto> GetRequestBudgetAsync(ReimbursementRequest request, decimal? amountOverride = null)
        {
            var now = DateTime.UtcNow;
            var amount = amountOverride ?? request.ApprovedAmount ?? request.TotalAmount;
            var result = new ReimbursementBudgetDto
            {
                ReimbursementId = request.Id,
                RequestAmount = amount,
                CalculationNote =
                    $"Budget year {now.Year} (January to December) and month {now.ToString("MMMM yyyy", Money)}. " +
                    "Remaining = budget minus paid minus approved-but-unpaid. Pending requests (including this one) are shown " +
                    "separately and are not treated as spent. The projection shows the position if this request is approved now."
            };

            if (request.Classification == ReimbursementClassification.PersonalUse)
            {
                result.IsPersonalUse = true;
                result.Note = "Personal Use: not charged to any department budget.";
                return result;
            }

            if (request.DepartmentId == null)
            {
                result.NeedsDepartmentLink = true;
                result.Note = $"This older request only has the typed department \"{request.Department}\". " +
                              "Link it to a department to see budget figures.";
                return result;
            }

            var budget = await GetDepartmentBudgetAsync(request.DepartmentId.Value, now.Year, now.Month);
            if (budget == null)
            {
                result.Note = "The department on this request could not be found.";
                return result;
            }

            result.Department = budget;
            result.AlreadyCommitted = request.Status is ReimbursementStatus.Approved or ReimbursementStatus.Paid;

            if (result.AlreadyCommitted)
            {
                result.ProjectedRemainingThisYear = budget.RemainingThisYear;
                result.ProjectedUtilisationPercentThisYear = budget.UtilisationPercentThisYear;
                result.ProjectedRemainingThisMonth = budget.RemainingThisMonth;
                result.ProjectedUtilisationPercentThisMonth = budget.UtilisationPercentThisMonth;
                result.Alerts = budget.Alerts;
                return result;
            }

            result.ProjectedRemainingThisYear = budget.RemainingThisYear - amount;
            result.ProjectedUtilisationPercentThisYear =
                Percent(budget.PaidThisYear + budget.CommittedThisYear + amount, budget.AnnualBudget);
            result.WouldExceedAnnual = result.ProjectedRemainingThisYear < 0;

            if (budget.MonthlyBudget.HasValue)
            {
                result.ProjectedRemainingThisMonth = budget.RemainingThisMonth - amount;
                result.ProjectedUtilisationPercentThisMonth =
                    Percent(budget.PaidThisMonth + budget.CommittedThisMonth + amount, budget.MonthlyBudget.Value);
                result.WouldExceedMonthly = result.ProjectedRemainingThisMonth < 0;
            }

            result.RequiresOverBudgetJustification = result.WouldExceedAnnual || result.WouldExceedMonthly;

            var alerts = new List<BudgetAlertDto>();
            if (result.WouldExceedAnnual)
            {
                alerts.Add(new BudgetAlertDto
                {
                    Code = "WouldExceedAnnual",
                    Severity = "danger",
                    Message = $"Approving {Format(amount)} would take {budget.DepartmentName} over its {budget.Year} budget " +
                              $"by {Format(-(result.ProjectedRemainingThisYear ?? 0))}."
                });
            }

            if (result.WouldExceedMonthly)
            {
                alerts.Add(new BudgetAlertDto
                {
                    Code = "WouldExceedMonthly",
                    Severity = "warning",
                    Message = $"Approving {Format(amount)} would take {budget.DepartmentName} over its monthly guide " +
                              $"of {Format(budget.MonthlyBudget ?? 0)} by {Format(-(result.ProjectedRemainingThisMonth ?? 0))}."
                });
            }

            alerts.AddRange(budget.Alerts);
            result.Alerts = alerts;
            return result;
        }

        private List<BudgetAlertDto> BuildDepartmentAlerts(DepartmentBudgetDto b)
        {
            var alerts = new List<BudgetAlertDto>();
            var used = b.PaidThisYear + b.CommittedThisYear;

            if ((b.AnnualBudget > 0 && used > b.AnnualBudget) || (b.AnnualBudget == 0 && used > 0))
            {
                alerts.Add(new BudgetAlertDto
                {
                    Code = "OverAnnual",
                    Severity = "danger",
                    Message = $"{b.DepartmentName} has paid or committed {Format(used)} against a {b.Year} budget of " +
                              $"{Format(b.AnnualBudget)}, which is {Format(used - b.AnnualBudget)} over."
                });
            }
            else if (b.UtilisationPercentThisYear.HasValue && b.UtilisationPercentThisYear.Value >= _options.WarningThresholdPercent)
            {
                alerts.Add(new BudgetAlertDto
                {
                    Code = "NearAnnualLimit",
                    Severity = "warning",
                    Message = $"{b.DepartmentName} has used {b.UtilisationPercentThisYear:0.#}% of its {b.Year} budget " +
                              $"({Format(b.RemainingThisYear)} left)."
                });
            }

            if (b.MonthlyBudget.HasValue && b.RemainingThisMonth < 0)
            {
                alerts.Add(new BudgetAlertDto
                {
                    Code = "OverMonthly",
                    Severity = "warning",
                    Message = $"{b.DepartmentName} is {Format(-(b.RemainingThisMonth ?? 0))} over its monthly guide of " +
                              $"{Format(b.MonthlyBudget.Value)} this month."
                });
            }

            if (b.PendingThisYear > 0)
            {
                if (b.RemainingThisYear <= 0)
                {
                    alerts.Add(new BudgetAlertDto
                    {
                        Code = "HighPending",
                        Severity = "warning",
                        Message = $"{Format(b.PendingThisYear)} is waiting for review or approval, but nothing is left in the budget."
                    });
                }
                else if (b.PendingThisYear * 100m / b.RemainingThisYear >= _options.HighPendingSharePercent)
                {
                    alerts.Add(new BudgetAlertDto
                    {
                        Code = "HighPending",
                        Severity = "info",
                        Message = $"Pending requests ({Format(b.PendingThisYear)}) equal " +
                                  $"{Math.Round(b.PendingThisYear * 100m / b.RemainingThisYear, 0)}% of what is left this year."
                    });
                }
            }

            return alerts;
        }

        private static bool InMonth(DateTime date, int year, int month) => date.Year == year && date.Month == month;

        private static decimal? Percent(decimal part, decimal whole) =>
            whole > 0 ? Math.Round(part * 100m / whole, 1, MidpointRounding.AwayFromZero) : null;

        private static string Format(decimal amount) => amount.ToString("C", Money);
    }
}
