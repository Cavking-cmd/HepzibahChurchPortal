using System.Globalization;
using ChurchPortal.Core.Dtos;
using ChurchPortal.Core.Dtos.Reports;
using ChurchPortal.Core.Entities;
using ChurchPortal.Repositories.Interfaces;
using ChurchPortal.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace ChurchPortal.Services.Implementations
{
    /// <summary>
    /// Financial reports. All totals are calculated here, on the server, from one set of
    /// rows and one set of rules (see <see cref="BudgetService"/>), so dashboards, tables,
    /// charts and CSV exports always show the same numbers.
    ///
    /// Amounts:  requested = request total; committed = approved amount, else total;
    ///           paid = amount actually paid.
    /// Dates:    paid is counted on the payment date entered at payment confirmation;
    ///           approved on the approval date; requested on the first submission date.
    /// Rejected, returned and draft requests are never counted as spending.
    /// </summary>
    public class FinancialReportService : IFinancialReportService
    {
        private static readonly CultureInfo Labels = CultureInfo.GetCultureInfo("en-CA");

        private readonly IReimbursementRepository _reimbursementRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IBudgetService _budget;
        private readonly ILogger<FinancialReportService> _logger;

        public FinancialReportService(
            IReimbursementRepository reimbursementRepository,
            IDepartmentRepository departmentRepository,
            IBudgetService budgetService,
            ILogger<FinancialReportService> logger)
        {
            _reimbursementRepository = reimbursementRepository;
            _departmentRepository = departmentRepository;
            _budget = budgetService;
            _logger = logger;
        }

        // =====================================================================
        // Summary dashboard
        // =====================================================================

        public async Task<BaseResponse<FinanceSummaryDto>> GetSummaryAsync(FinancialReportFilter filter)
        {
            try
            {
                var now = DateTime.UtcNow;
                var from = (filter.From ?? new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc)).Date;
                var to = (filter.To ?? now).Date;
                if (to < from)
                {
                    (from, to) = (to, from);
                }

                var allRows = await _reimbursementRepository.GetReportRowsAsync();
                var rows = ApplyFilter(allRows, filter);
                bool InRange(DateTime d) => d >= from && d < to.AddDays(1);

                var paid = rows.Where(r => r.Status == ReimbursementStatus.Paid && InRange(_budget.PaidOn(r))).ToList();
                var submitted = rows.Where(r => InRange(_budget.SubmittedOn(r))).ToList();
                var approved = rows.Where(r => WasApproved(r) && r.ApprovedDate.HasValue && InRange(r.ApprovedDate.Value)).ToList();
                var rejected = rows.Where(r => r.Status == ReimbursementStatus.Rejected && InRange(r.RejectedDate ?? _budget.SubmittedOn(r))).ToList();

                var dto = new FinanceSummaryDto
                {
                    From = from,
                    To = to,
                    PaidInPeriod = paid.Sum(r => r.Paid),
                    PaidDepartmentInPeriod = paid.Where(r => !r.IsPersonalUse).Sum(r => r.Paid),
                    PaidPersonalUseInPeriod = paid.Where(r => r.IsPersonalUse).Sum(r => r.Paid),
                    PaidCountInPeriod = paid.Count,
                    AveragePaidInPeriod = paid.Count == 0 ? 0 : Round(paid.Average(r => r.Paid)),
                    SubmittedCountInPeriod = submitted.Count,
                    RequestedInPeriod = submitted.Sum(r => r.Requested),
                    ApprovedCountInPeriod = approved.Count,
                    ApprovedInPeriod = approved.Sum(r => r.Committed),
                    RejectedCountInPeriod = rejected.Count,
                    ApprovedNotPaidNow = rows.Where(r => r.Status == ReimbursementStatus.Approved).Sum(r => r.Committed),
                    ApprovedNotPaidCountNow = rows.Count(r => r.Status == ReimbursementStatus.Approved),
                    PendingApprovalNow = rows.Where(r => r.Status == ReimbursementStatus.PendingApproval).Sum(r => r.Requested),
                    PendingApprovalCountNow = rows.Count(r => r.Status == ReimbursementStatus.PendingApproval),
                    UnderReviewNow = rows.Where(r => IsUnderReview(r.Status)).Sum(r => r.Requested),
                    UnderReviewCountNow = rows.Count(r => IsUnderReview(r.Status)),
                    ReturnedForCorrectionNow = rows.Where(r => r.Status == ReimbursementStatus.ReturnedForCorrection).Sum(r => r.Requested),
                    ReturnedForCorrectionCountNow = rows.Count(r => r.Status == ReimbursementStatus.ReturnedForCorrection),
                    UnlinkedLegacyRequestCount = rows.Count(r => !r.IsPersonalUse && r.DepartmentId == null),
                    WarningThresholdPercent = _budget.WarningThresholdPercent,
                    BudgetYear = to.Year
                };

                // Budget figures ignore the status/member filters (they would make a budget meaningless)
                // but respect the department filter.
                var departments = (await _departmentRepository.GetAllOrderedAsync(activeOnly: true))
                    .Where(d => filter.DepartmentId == null || d.Id == filter.DepartmentId)
                    .ToList();
                if (filter.Classification != "personal")
                {
                    var budgetMonth = to.Year == now.Year ? now.Month : 12;
                    var budgets = departments.Select(d => _budget.Calculate(d, allRows, to.Year, budgetMonth)).ToList();
                    dto.ActiveDepartmentCount = budgets.Count;
                    dto.TotalActiveAnnualBudget = budgets.Sum(b => b.AnnualBudget);
                    dto.DepartmentPaidThisYear = budgets.Sum(b => b.PaidThisYear);
                    dto.DepartmentCommittedThisYear = budgets.Sum(b => b.CommittedThisYear);
                    dto.BudgetUtilisationPercent = Percent(dto.DepartmentPaidThisYear + dto.DepartmentCommittedThisYear, dto.TotalActiveAnnualBudget);
                    dto.DepartmentsOverBudget = budgets.Count(b => b.Alerts.Any(a => a.Code == "OverAnnual"));
                    dto.DepartmentsNearLimit = budgets.Count(b => b.Alerts.Any(a => a.Code == "NearAnnualLimit"));
                }

                dto.Definitions =
                [
                    $"Period {from:yyyy-MM-dd} to {to:yyyy-MM-dd}.",
                    "Paid: amount actually paid, counted on the payment date recorded when payment was confirmed.",
                    "Approved in period: requests given final approval in the period (whether or not paid since), at the approved amount.",
                    "Requested in period: requests first submitted in the period, at the requested total, whatever happened next.",
                    "Approved not paid, Pending approval, Under review: what is sitting in each stage right now, regardless of period.",
                    $"Budget utilisation: (paid + approved-not-paid) for active departments in {to.Year}, divided by their total annual budget. Personal Use is excluded.",
                    $"Near limit: at or above {_budget.WarningThresholdPercent:0.#}% of annual budget. Over budget: paid + committed above the annual budget.",
                    "Rejected and returned requests are never counted as spending."
                ];

                return Ok(dto);
            }
            catch (Exception ex)
            {
                return Unexpected<FinanceSummaryDto>(ex, "build the finance summary");
            }
        }

        // =====================================================================
        // Monthly expenditure
        // =====================================================================

        public async Task<BaseResponse<MonthlyExpenditureDto>> GetMonthlyAsync(int year, int month, FinancialReportFilter filter)
        {
            try
            {
                if (month is < 1 or > 12 || year is < 2000 or > 2100)
                {
                    var now = DateTime.UtcNow;
                    year = now.Year;
                    month = now.Month;
                }

                var rows = ApplyFilter(await _reimbursementRepository.GetReportRowsAsync(), filter);
                var start = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
                var end = start.AddMonths(1);
                var prevStart = start.AddMonths(-1);
                bool In(DateTime d, DateTime a, DateTime b) => d >= a && d < b;

                var paid = rows.Where(r => r.Status == ReimbursementStatus.Paid && In(_budget.PaidOn(r), start, end)).ToList();
                var prevPaid = rows.Where(r => r.Status == ReimbursementStatus.Paid && In(_budget.PaidOn(r), prevStart, start)).Sum(r => r.Paid);
                var approved = rows.Where(r => WasApproved(r) && r.ApprovedDate.HasValue && In(r.ApprovedDate.Value, start, end)).ToList();
                var requested = rows.Where(r => In(_budget.SubmittedOn(r), start, end)).ToList();

                // Approved by month end and not yet paid at month end.
                var awaiting = rows.Where(r =>
                        r.ApprovedDate.HasValue && r.ApprovedDate.Value < end &&
                        (r.Status == ReimbursementStatus.Approved ||
                         (r.Status == ReimbursementStatus.Paid && _budget.PaidOn(r) >= end)))
                    .ToList();

                var paidTotal = paid.Sum(r => r.Paid);
                var largest = paid.OrderByDescending(r => r.Paid).FirstOrDefault();

                var dto = new MonthlyExpenditureDto
                {
                    Year = year,
                    Month = month,
                    MonthLabel = start.ToString("MMMM yyyy", Labels),
                    PaidTotal = paidTotal,
                    PaidCount = paid.Count,
                    AveragePaid = paid.Count == 0 ? 0 : Round(paidTotal / paid.Count),
                    LargestPaid = largest == null ? null : new LargestPaymentDto
                    {
                        Id = largest.Id,
                        RequestNumber = largest.RequestNumber,
                        PayeeName = largest.PayeeName,
                        Department = largest.Label,
                        Amount = largest.Paid,
                        PaymentDate = largest.PaymentDate
                    },
                    ApprovedTotal = approved.Sum(r => r.Committed),
                    ApprovedCount = approved.Count,
                    RequestedTotal = requested.Sum(r => r.Requested),
                    RequestedCount = requested.Count,
                    AwaitingPaymentCount = awaiting.Count,
                    AwaitingPaymentAmount = awaiting.Sum(r => r.Committed),
                    PreviousMonthPaidTotal = prevPaid,
                    PaidChangePercent = prevPaid > 0 ? Round((paidTotal - prevPaid) * 100m / prevPaid) : null,
                    PaidDepartmentTotal = paid.Where(r => !r.IsPersonalUse).Sum(r => r.Paid),
                    PaidPersonalUseTotal = paid.Where(r => r.IsPersonalUse).Sum(r => r.Paid),
                    PaidByDepartment = paid
                        .GroupBy(r => r.GroupKey)
                        .Select(g => new LabelAmountDto
                        {
                            Key = g.Key,
                            Label = g.First().Label,
                            IsPersonalUse = g.First().IsPersonalUse,
                            Amount = g.Sum(r => r.Paid),
                            Count = g.Count()
                        })
                        .OrderByDescending(x => x.Amount)
                        .ToList(),
                    Definitions =
                    [
                        "Paid: amount actually paid, by the payment date recorded at payment confirmation.",
                        "Approved: requests given final approval this month, at the approved amount.",
                        "Requested: requests first submitted this month, at the requested total.",
                        "Awaiting payment: approved by the end of the month and not paid by then.",
                        "Change: this month's paid total compared with last month's."
                    ]
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                return Unexpected<MonthlyExpenditureDto>(ex, "build the monthly report");
            }
        }

        // =====================================================================
        // Department spending
        // =====================================================================

        public async Task<BaseResponse<DepartmentSpendingReportDto>> GetDepartmentSpendingAsync(int year, FinancialReportFilter filter)
        {
            try
            {
                var now = DateTime.UtcNow;
                if (year is < 2000 or > 2100) year = now.Year;
                var budgetMonth = year == now.Year ? now.Month : 12;

                var allRows = await _reimbursementRepository.GetReportRowsAsync();
                var rows = ApplyFilter(allRows, filter);
                var departments = (await _departmentRepository.GetAllOrderedAsync(activeOnly: false))
                    .Where(d => filter.DepartmentId == null || d.Id == filter.DepartmentId)
                    .ToList();

                var result = new List<DepartmentSpendingDto>();

                if (filter.Classification != "personal")
                {
                    foreach (var d in departments)
                    {
                        var b = _budget.Calculate(d, rows, year, budgetMonth);
                        var mine = rows.Where(r => r.DepartmentId == d.Id && !r.IsPersonalUse).ToList();
                        var row = BuildSpendingRow(d.Id.ToString(), d.Id, d.Name, false, false, d.IsActive, mine, year);
                        row.AnnualBudget = d.AnnualBudget;
                        row.MonthlyBudget = d.MonthlyBudget;
                        row.Remaining = b.RemainingThisYear;
                        row.UtilisationPercent = b.UtilisationPercentThisYear;
                        row.IsOverBudget = b.Alerts.Any(a => a.Code == "OverAnnual");
                        row.IsNearLimit = b.Alerts.Any(a => a.Code == "NearAnnualLimit");
                        row.HasHighPending = b.Alerts.Any(a => a.Code == "HighPending");

                        // Skip inactive departments with nothing to show for the year.
                        if (!d.IsActive && row.RequestCount == 0 && row.Paid == 0 && row.Committed == 0 && row.Pending == 0)
                        {
                            continue;
                        }

                        result.Add(row);
                    }

                    if (filter.DepartmentId == null)
                    {
                        foreach (var legacy in rows.Where(r => !r.IsPersonalUse && r.DepartmentId == null).GroupBy(r => r.GroupKey))
                        {
                            var list = legacy.ToList();
                            var row = BuildSpendingRow(legacy.Key, null, list[0].Label, false, true, true, list, year);
                            if (row.RequestCount + row.PaidCount > 0 || row.Committed > 0 || row.Pending > 0)
                            {
                                result.Add(row);
                            }
                        }
                    }
                }

                if (filter.Classification != "department" && filter.DepartmentId == null)
                {
                    var personal = rows.Where(r => r.IsPersonalUse).ToList();
                    var row = BuildSpendingRow("personal", null, ReimbursementService.PersonalUseLabel, true, false, true, personal, year);
                    result.Add(row);
                }

                var budgeted = result.Where(r => !r.IsPersonalUse && !r.IsLegacyUnlinked).ToList();
                var topPaid = budgeted.Where(r => r.Paid > 0).OrderByDescending(r => r.Paid).FirstOrDefault();
                if (topPaid != null) topPaid.IsHighestPaid = true;
                var topUtil = budgeted.Where(r => r.UtilisationPercent.HasValue).OrderByDescending(r => r.UtilisationPercent).FirstOrDefault();
                if (topUtil != null && topUtil.UtilisationPercent > 0) topUtil.IsHighestUtilisation = true;

                var dto = new DepartmentSpendingReportDto
                {
                    Year = year,
                    WarningThresholdPercent = _budget.WarningThresholdPercent,
                    Rows = result.OrderBy(r => r.IsPersonalUse).ThenBy(r => r.IsLegacyUnlinked).ThenByDescending(r => r.Paid).ThenBy(r => r.Department).ToList(),
                    DepartmentPaidTotal = result.Where(r => !r.IsPersonalUse).Sum(r => r.Paid),
                    PersonalUsePaidTotal = result.Where(r => r.IsPersonalUse).Sum(r => r.Paid),
                    Definitions =
                    [
                        $"Calendar year {year} (January to December).",
                        "Paid: paid in the year, by payment date. Committed: approved and not yet paid, by approval date. Pending: under review or pending approval, by submission date.",
                        "Remaining = annual budget - paid - committed. Pending is shown separately and not subtracted.",
                        "Utilisation = (paid + committed) / annual budget.",
                        "Requests: submitted in the year. Average and largest: of payments made in the year.",
                        "Personal Use is never charged to a department. Rows marked \"not linked\" are older requests with typed department names.",
                        "A high total does not by itself mean a department is overspending; compare it with its budget and utilisation."
                    ]
                };
                dto.PaidTotal = dto.DepartmentPaidTotal + dto.PersonalUsePaidTotal;

                return Ok(dto);
            }
            catch (Exception ex)
            {
                return Unexpected<DepartmentSpendingReportDto>(ex, "build the department report");
            }
        }

        private DepartmentSpendingDto BuildSpendingRow(
            string key, Guid? departmentId, string name, bool personal, bool legacy, bool active,
            List<ReimbursementReportRow> rows, int year)
        {
            var paid = rows.Where(r => r.Status == ReimbursementStatus.Paid && _budget.PaidOn(r).Year == year).ToList();
            var committed = rows.Where(r => r.Status == ReimbursementStatus.Approved && _budget.CommittedOn(r).Year == year).Sum(r => r.Committed);
            var pending = rows.Where(r => _budget.IsPending(r.Status) && _budget.SubmittedOn(r).Year == year).Sum(r => r.Requested);

            return new DepartmentSpendingDto
            {
                Key = key,
                DepartmentId = departmentId,
                Department = name,
                IsPersonalUse = personal,
                IsLegacyUnlinked = legacy,
                IsActive = active,
                Paid = paid.Sum(r => r.Paid),
                Committed = committed,
                Pending = pending,
                RequestCount = rows.Count(r => _budget.SubmittedOn(r).Year == year),
                PaidCount = paid.Count,
                AveragePaid = paid.Count == 0 ? 0 : Round(paid.Average(r => r.Paid)),
                LargestPaid = paid.Count == 0 ? 0 : paid.Max(r => r.Paid),
                RejectedCount = rows.Count(r => r.Status == ReimbursementStatus.Rejected && (r.RejectedDate ?? _budget.SubmittedOn(r)).Year == year)
            };
        }

        // =====================================================================
        // Trends
        // =====================================================================

        public async Task<BaseResponse<SpendingTrendsDto>> GetTrendsAsync(int months, FinancialReportFilter filter)
        {
            try
            {
                if (months is < 1 or > 36) months = 12;
                var now = DateTime.UtcNow;
                var first = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-(months - 1));

                var rows = ApplyFilter(await _reimbursementRepository.GetReportRowsAsync(), filter);
                var departments = await _departmentRepository.GetAllOrderedAsync(activeOnly: true);
                var monthStarts = Enumerable.Range(0, months).Select(i => first.AddMonths(i)).ToList();

                var points = new List<TrendPointDto>();
                foreach (var start in monthStarts)
                {
                    var end = start.AddMonths(1);
                    var paid = rows.Where(r => r.Status == ReimbursementStatus.Paid && _budget.PaidOn(r) >= start && _budget.PaidOn(r) < end).ToList();
                    var submitted = rows.Where(r => _budget.SubmittedOn(r) >= start && _budget.SubmittedOn(r) < end).ToList();

                    // Year-to-date utilisation at the end of this month, for departments.
                    var yearStart = new DateTime(start.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    var totalBudget = departments.Where(d => filter.DepartmentId == null || d.Id == filter.DepartmentId).Sum(d => d.AnnualBudget);
                    var ytdPaid = rows.Where(r => !r.IsPersonalUse && r.DepartmentId != null && r.Status == ReimbursementStatus.Paid
                                                  && _budget.PaidOn(r) >= yearStart && _budget.PaidOn(r) < end).Sum(r => r.Paid);
                    var ytdCommitted = rows.Where(r => !r.IsPersonalUse && r.DepartmentId != null && r.Status == ReimbursementStatus.Approved
                                                       && _budget.CommittedOn(r) >= yearStart && _budget.CommittedOn(r) < end).Sum(r => r.Committed);

                    points.Add(new TrendPointDto
                    {
                        Year = start.Year,
                        Month = start.Month,
                        MonthLabel = start.ToString("MMM yyyy", Labels),
                        Paid = paid.Sum(r => r.Paid),
                        PaidDepartment = paid.Where(r => !r.IsPersonalUse).Sum(r => r.Paid),
                        PaidPersonalUse = paid.Where(r => r.IsPersonalUse).Sum(r => r.Paid),
                        PaidCount = paid.Count,
                        AveragePaid = paid.Count == 0 ? 0 : Round(paid.Average(r => r.Paid)),
                        SubmittedCount = submitted.Count,
                        Requested = submitted.Sum(r => r.Requested),
                        YearToDateUtilisationPercent = filter.Classification == "personal" ? null : Percent(ytdPaid + ytdCommitted, totalBudget)
                    });
                }

                var windowEnd = first.AddMonths(months);
                var paidInWindow = rows.Where(r => r.Status == ReimbursementStatus.Paid && _budget.PaidOn(r) >= first && _budget.PaidOn(r) < windowEnd).ToList();
                var series = paidInWindow
                    .GroupBy(r => r.GroupKey)
                    .Select(g => new { g.Key, Label = g.First().Label, Total = g.Sum(r => r.Paid), Rows = g.ToList() })
                    .OrderByDescending(g => g.Total)
                    .Take(8)
                    .Select(g => new DepartmentTrendSeriesDto
                    {
                        Key = g.Key,
                        Label = g.Label,
                        PaidByMonth = monthStarts
                            .Select(s => g.Rows.Where(r => _budget.PaidOn(r) >= s && _budget.PaidOn(r) < s.AddMonths(1)).Sum(r => r.Paid))
                            .ToList()
                    })
                    .ToList();

                return Ok(new SpendingTrendsDto
                {
                    Points = points,
                    ByDepartment = series,
                    Definitions =
                    [
                        "Paid per month: by payment date. Requests per month: by first submission date.",
                        "Average: average payment in that month.",
                        "Year-to-date utilisation: department payments from January to that month plus approvals still unpaid, over the total annual budget of active departments. Personal Use excluded.",
                        "Department lines show the eight largest by amount paid over the period."
                    ]
                });
            }
            catch (Exception ex)
            {
                return Unexpected<SpendingTrendsDto>(ex, "build spending trends");
            }
        }

        // =====================================================================
        // Workflow and processing times
        // =====================================================================

        public async Task<BaseResponse<WorkflowReportDto>> GetWorkflowAsync(int staleDays, FinancialReportFilter filter)
        {
            try
            {
                if (staleDays is < 1 or > 365) staleDays = 14;
                var now = DateTime.UtcNow;

                var rows = ApplyFilter(await _reimbursementRepository.GetReportRowsAsync(), filter);
                var transitions = (await _reimbursementRepository.GetTransitionsAsync())
                    .Select(t => new ReimbursementTransitionRow
                    {
                        RequestId = t.RequestId,
                        FromStatus = t.FromStatus.HasValue ? Norm(t.FromStatus.Value) : null,
                        ToStatus = Norm(t.ToStatus),
                        Timestamp = t.Timestamp
                    })
                    .Where(t => t.FromStatus != t.ToStatus)
                    .GroupBy(t => t.RequestId)
                    .ToDictionary(g => g.Key, g => g.OrderBy(t => t.Timestamp).ToList());

                var reviewDurations = new List<double>();
                var toApproval = new List<double>();
                var toPayment = new List<double>();
                var awaiting = new List<AwaitingActionDto>();

                foreach (var r in rows)
                {
                    transitions.TryGetValue(r.Id, out var steps);
                    steps ??= [];

                    // Time spent in Under Review: completed spells only, summed if it was resubmitted.
                    double reviewDays = 0;
                    var completedSpells = 0;
                    DateTime? enteredReview = null;
                    foreach (var s in steps)
                    {
                        if (s.ToStatus == ReimbursementStatus.UnderReview)
                        {
                            enteredReview = s.Timestamp;
                        }
                        else if (enteredReview.HasValue && s.FromStatus == ReimbursementStatus.UnderReview)
                        {
                            reviewDays += (s.Timestamp - enteredReview.Value).TotalDays;
                            completedSpells++;
                            enteredReview = null;
                        }
                    }

                    if (completedSpells > 0)
                    {
                        reviewDurations.Add(reviewDays);
                    }

                    var firstSubmitted = steps.FirstOrDefault(s => s.ToStatus == ReimbursementStatus.UnderReview)?.Timestamp ?? r.SubmittedDate;
                    var firstApproved = steps.FirstOrDefault(s => s.ToStatus == ReimbursementStatus.Approved)?.Timestamp ?? r.ApprovedDate;
                    var firstPaid = steps.FirstOrDefault(s => s.ToStatus == ReimbursementStatus.Paid)?.Timestamp;

                    if (firstSubmitted.HasValue && firstApproved.HasValue && firstApproved >= firstSubmitted)
                    {
                        toApproval.Add((firstApproved.Value - firstSubmitted.Value).TotalDays);
                    }

                    if (firstApproved.HasValue && firstPaid.HasValue && firstPaid >= firstApproved)
                    {
                        toPayment.Add((firstPaid.Value - firstApproved.Value).TotalDays);
                    }

                    var current = Norm(r.Status);
                    var waitingFor = current switch
                    {
                        ReimbursementStatus.UnderReview => "Accounts Officer review",
                        ReimbursementStatus.PendingApproval => "Accounts Admin approval",
                        ReimbursementStatus.Approved => "Payment confirmation",
                        ReimbursementStatus.ReturnedForCorrection => "Member correction",
                        _ => null
                    };

                    if (waitingFor != null)
                    {
                        var since = steps.LastOrDefault(s => s.ToStatus == current)?.Timestamp
                                    ?? (current == ReimbursementStatus.Approved ? r.ApprovedDate : null)
                                    ?? r.SubmittedDate
                                    ?? r.CreatedDate;
                        var days = Math.Round((now - since).TotalDays, 1);
                        awaiting.Add(new AwaitingActionDto
                        {
                            Id = r.Id,
                            RequestNumber = r.RequestNumber,
                            PayeeName = r.PayeeName,
                            Department = r.Label,
                            Status = current,
                            StatusName = StatusLabel(current),
                            WaitingFor = waitingFor,
                            Amount = current == ReimbursementStatus.Approved ? r.Committed : r.Requested,
                            InStatusSince = since,
                            DaysInStatus = days,
                            IsStale = days > staleDays
                        });
                    }
                }

                var order = new[]
                {
                    ReimbursementStatus.UnderReview, ReimbursementStatus.PendingApproval, ReimbursementStatus.Approved,
                    ReimbursementStatus.Paid, ReimbursementStatus.Rejected, ReimbursementStatus.ReturnedForCorrection
                };

                var dto = new WorkflowReportDto
                {
                    StaleThresholdDays = staleDays,
                    ByStatus = order.Select(s =>
                    {
                        var inStatus = rows.Where(r => Norm(r.Status) == s).ToList();
                        return new StatusBucketDto
                        {
                            Status = s,
                            StatusName = StatusLabel(s),
                            Count = inStatus.Count,
                            Amount = s switch
                            {
                                ReimbursementStatus.Paid => inStatus.Sum(r => r.Paid),
                                ReimbursementStatus.Approved => inStatus.Sum(r => r.Committed),
                                _ => inStatus.Sum(r => r.Requested)
                            }
                        };
                    }).ToList(),
                    AwaitingAction = awaiting.OrderByDescending(a => a.DaysInStatus).ToList(),
                    StaleCount = awaiting.Count(a => a.IsStale),
                    AverageDaysUnderReview = Avg(reviewDurations),
                    UnderReviewSampleSize = reviewDurations.Count,
                    AverageDaysSubmissionToApproval = Avg(toApproval),
                    SubmissionToApprovalSampleSize = toApproval.Count,
                    AverageDaysApprovalToPayment = Avg(toPayment),
                    ApprovalToPaymentSampleSize = toPayment.Count,
                    Definitions =
                    [
                        "Counts and amounts show where requests are right now. Paid uses amount paid; Approved uses the approved amount; others use the requested total.",
                        "Time under review: from entering Under Review until the Accounts Officer forwards, returns or rejects it. If a request was returned and resubmitted, each spell is added together.",
                        "Submission to approval: from first submission to final approval. Approval to payment: from final approval to payment confirmation.",
                        "Times come from the request history. Older requests created before history was recorded are left out rather than guessed.",
                        $"Stale: waiting in its current stage for more than {staleDays} days."
                    ]
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                return Unexpected<WorkflowReportDto>(ex, "build the workflow report");
            }
        }

        // =====================================================================
        // Forecast and risk (estimates)
        // =====================================================================

        public async Task<BaseResponse<FinanceForecastDto>> GetForecastAsync(int year, FinancialReportFilter filter)
        {
            try
            {
                var now = DateTime.UtcNow;
                if (year is < 2000 or > 2100) year = now.Year;
                var monthsElapsed = year == now.Year ? now.Month : year < now.Year ? 12 : 0;

                var rows = ApplyFilter(await _reimbursementRepository.GetReportRowsAsync(), filter);
                var departments = (await _departmentRepository.GetAllOrderedAsync(activeOnly: true))
                    .Where(d => filter.DepartmentId == null || d.Id == filter.DepartmentId)
                    .ToList();

                var thisMonthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                var last3Start = thisMonthStart.AddMonths(-2);
                var prev3Start = thisMonthStart.AddMonths(-5);
                var nextMonth = thisMonthStart.AddMonths(1);

                var forecasts = new List<DepartmentForecastDto>();
                if (filter.Classification != "personal")
                {
                    foreach (var d in departments)
                    {
                        var mine = rows.Where(r => r.DepartmentId == d.Id && !r.IsPersonalUse).ToList();
                        var paidRows = mine.Where(r => r.Status == ReimbursementStatus.Paid && _budget.PaidOn(r).Year == year).ToList();
                        var paidYtd = paidRows.Sum(r => r.Paid);
                        var committed = mine.Where(r => r.Status == ReimbursementStatus.Approved && _budget.CommittedOn(r).Year == year).Sum(r => r.Committed);
                        var pending = mine.Where(r => _budget.IsPending(r.Status) && _budget.SubmittedOn(r).Year == year).Sum(r => r.Requested);
                        var last3 = mine.Where(r => r.Status == ReimbursementStatus.Paid && _budget.PaidOn(r) >= last3Start && _budget.PaidOn(r) < nextMonth).Sum(r => r.Paid);
                        var prev3 = mine.Where(r => r.Status == ReimbursementStatus.Paid && _budget.PaidOn(r) >= prev3Start && _budget.PaidOn(r) < last3Start).Sum(r => r.Paid);

                        var f = new DepartmentForecastDto
                        {
                            Key = d.Id.ToString(),
                            DepartmentId = d.Id,
                            Department = d.Name,
                            AnnualBudget = d.AnnualBudget,
                            PaidYearToDate = paidYtd,
                            Committed = committed,
                            Pending = pending,
                            RemainingIfPendingApproved = d.AnnualBudget - paidYtd - committed - pending,
                            LastThreeMonthsPaid = last3,
                            PreviousThreeMonthsPaid = prev3,
                            IsIncreasing = prev3 > 0 && last3 > prev3 * 1.2m
                        };
                        f.WouldOverrunIfPendingApproved = f.RemainingIfPendingApproved < 0;

                        var enoughData = monthsElapsed >= 3 && paidRows.Count >= 3;
                        f.IsReliable = enoughData;
                        if (!enoughData)
                        {
                            f.ReliabilityNote = monthsElapsed < 3
                                ? "Too early in the year for a projection (fewer than 3 months)."
                                : "Not enough payments this year (fewer than 3) for a projection.";
                        }
                        else
                        {
                            var monthlyRate = (paidYtd + committed) / monthsElapsed;
                            f.ProjectedYearEnd = Round(monthlyRate * 12);
                            f.ProjectedOverrun = f.ProjectedYearEnd > d.AnnualBudget ? Round(f.ProjectedYearEnd.Value - d.AnnualBudget) : null;

                            var remaining = d.AnnualBudget - paidYtd - committed;
                            if (remaining <= 0)
                            {
                                f.ProjectedExhaustionMonth = "Already used up";
                            }
                            else if (monthlyRate > 0)
                            {
                                var monthIndex = monthsElapsed + (int)Math.Ceiling(remaining / monthlyRate);
                                f.ProjectedExhaustionMonth = monthIndex <= 12
                                    ? new DateTime(year, monthIndex, 1).ToString("MMMM yyyy", Labels)
                                    : null;
                            }
                        }

                        forecasts.Add(f);
                    }
                }

                // Unusually large payments: more than two standard deviations above the
                // department's average, only where there are at least 5 payments to compare.
                var unusual = new List<UnusualReimbursementDto>();
                var yearAgo = now.AddMonths(-12);
                foreach (var group in rows.Where(r => r.Status == ReimbursementStatus.Paid && _budget.PaidOn(r) >= yearAgo).GroupBy(r => r.GroupKey))
                {
                    var amounts = group.Select(r => r.Paid).ToList();
                    if (amounts.Count < 5) continue;
                    var mean = amounts.Average();
                    var sd = (decimal)Math.Sqrt(amounts.Select(a => Math.Pow((double)(a - mean), 2)).Average());
                    if (sd == 0) continue;
                    foreach (var r in group.Where(x => x.Paid > mean + 2 * sd))
                    {
                        unusual.Add(new UnusualReimbursementDto
                        {
                            Id = r.Id,
                            RequestNumber = r.RequestNumber,
                            Department = r.Label,
                            PayeeName = r.PayeeName,
                            Amount = r.Paid,
                            DepartmentAverage = Round(mean),
                            Reason = $"More than twice the usual spread above this group's average of {Round(mean):0.00} (based on {amounts.Count} payments in the last 12 months)."
                        });
                    }
                }

                var paidThisMonth = rows.Where(r => r.Status == ReimbursementStatus.Paid && _budget.PaidOn(r) >= thisMonthStart && _budget.PaidOn(r) < nextMonth).Sum(r => r.Paid);
                var prevMonthStart = thisMonthStart.AddMonths(-1);
                var paidPrevMonth = rows.Where(r => r.Status == ReimbursementStatus.Paid && _budget.PaidOn(r) >= prevMonthStart && _budget.PaidOn(r) < thisMonthStart).Sum(r => r.Paid);
                var lastYearStart = thisMonthStart.AddYears(-1);
                var paidLastYearSameMonth = rows.Where(r => r.Status == ReimbursementStatus.Paid && _budget.PaidOn(r) >= lastYearStart && _budget.PaidOn(r) < lastYearStart.AddMonths(1)).Sum(r => r.Paid);

                return Ok(new FinanceForecastDto
                {
                    Year = year,
                    MonthsElapsed = monthsElapsed,
                    Departments = forecasts.OrderByDescending(f => f.ProjectedOverrun ?? 0).ThenBy(f => f.Department).ToList(),
                    UnusualReimbursements = unusual.OrderByDescending(u => u.Amount).ToList(),
                    OutstandingApprovedCommitments = rows.Where(r => r.Status == ReimbursementStatus.Approved).Sum(r => r.Committed),
                    PaidThisMonth = paidThisMonth,
                    PaidPreviousMonth = paidPrevMonth,
                    PaidSameMonthLastYear = paidLastYearSameMonth,
                    Definitions =
                    [
                        "These are estimates, not confirmed spending.",
                        "Projected year end = (paid + approved-not-paid so far this year) / months elapsed x 12. Only shown with at least 3 months and 3 payments.",
                        "Remaining if pending approved: annual budget minus paid, committed and everything still under review or pending approval.",
                        "Increasing: last 3 months paid is more than 20% above the 3 months before.",
                        "Unusual: a payment more than two standard deviations above its group's average over the last 12 months (needs at least 5 payments)."
                    ]
                });
            }
            catch (Exception ex)
            {
                return Unexpected<FinanceForecastDto>(ex, "build the forecast");
            }
        }

        // =====================================================================
        // Helpers
        // =====================================================================

        private static List<ReimbursementReportRow> ApplyFilter(IEnumerable<ReimbursementReportRow> rows, FinancialReportFilter f)
        {
            var query = rows;
            if (f.DepartmentId.HasValue) query = query.Where(r => r.DepartmentId == f.DepartmentId && !r.IsPersonalUse);
            if (f.Classification == "personal") query = query.Where(r => r.IsPersonalUse);
            if (f.Classification == "department") query = query.Where(r => !r.IsPersonalUse);
            if (f.Status.HasValue) query = query.Where(r => Norm(r.Status) == Norm(f.Status.Value));
            if (f.PayeeUserId.HasValue) query = query.Where(r => r.PayeeUserId == f.PayeeUserId);
            return query.ToList();
        }

        private static bool WasApproved(ReimbursementReportRow r) =>
            r.Status is ReimbursementStatus.Approved or ReimbursementStatus.Paid;

        private static bool IsUnderReview(ReimbursementStatus s) =>
            s is ReimbursementStatus.UnderReview or ReimbursementStatus.LegacyAccountsReview;

        private static ReimbursementStatus Norm(ReimbursementStatus s) =>
            s == ReimbursementStatus.LegacyAccountsReview ? ReimbursementStatus.UnderReview : s;

        private static string StatusLabel(ReimbursementStatus s) => s switch
        {
            ReimbursementStatus.UnderReview => "Under Review",
            ReimbursementStatus.PendingApproval => "Pending Approval",
            ReimbursementStatus.Approved => "Approved (awaiting payment)",
            ReimbursementStatus.Paid => "Paid",
            ReimbursementStatus.Rejected => "Rejected",
            ReimbursementStatus.ReturnedForCorrection => "Returned for Correction",
            ReimbursementStatus.Draft => "Draft",
            _ => s.ToString()
        };

        private static double? Avg(List<double> values) => values.Count == 0 ? null : Math.Round(values.Average(), 1);

        private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

        private static decimal? Percent(decimal part, decimal whole) =>
            whole > 0 ? Math.Round(part * 100m / whole, 1, MidpointRounding.AwayFromZero) : null;

        private static BaseResponse<T> Ok<T>(T data) => new() { Message = "Report generated.", Status = true, Data = data };

        private BaseResponse<T> Unexpected<T>(Exception ex, string action)
        {
            _logger.LogError(ex, "Failed to {Action}.", action);
            return new BaseResponse<T> { Message = $"Something went wrong while trying to {action}.", Status = false, Data = default };
        }
    }
}
