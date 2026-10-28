using ChurchPortal.Core.Entities;

namespace ChurchPortal.Core.Dtos.Reports
{
    // ---------------------------------------------------------------------
    // Internal rows loaded from the database for calculations (no receipts).
    // ---------------------------------------------------------------------

    public class ReimbursementReportRow
    {
        public Guid Id { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public Guid PayeeUserId { get; set; }
        public string PayeeName { get; set; } = string.Empty;
        public ReimbursementClassification Classification { get; set; }
        public Guid? DepartmentId { get; set; }
        public string DepartmentText { get; set; } = string.Empty;
        public string? DepartmentName { get; set; }
        public ReimbursementStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal? ApprovedAmount { get; set; }
        public decimal? AmountPaid { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? SubmittedDate { get; set; }
        public DateTime? ReviewedDate { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public DateTime? PaymentDate { get; set; }
        public DateTime? RejectedDate { get; set; }

        /// <summary>Amount requested by the member.</summary>
        public decimal Requested => TotalAmount;

        /// <summary>Amount committed once approved: the approved amount, else the requested total.</summary>
        public decimal Committed => ApprovedAmount ?? TotalAmount;

        /// <summary>Amount actually paid: recorded payment, else the committed amount.</summary>
        public decimal Paid => AmountPaid ?? Committed;

        public bool IsPersonalUse => Classification == ReimbursementClassification.PersonalUse;

        /// <summary>Display label used in breakdowns.</summary>
        public string Label => IsPersonalUse
            ? "Personal Use"
            : DepartmentName ?? (string.IsNullOrWhiteSpace(DepartmentText) ? "Unassigned" : $"{DepartmentText} (not linked)");

        /// <summary>Grouping key: department id, "personal", or "legacy:{text}".</summary>
        public string GroupKey => IsPersonalUse
            ? "personal"
            : DepartmentId?.ToString() ?? $"legacy:{DepartmentText.Trim().ToUpperInvariant()}";
    }

    public class ReimbursementTransitionRow
    {
        public Guid RequestId { get; set; }
        public ReimbursementStatus? FromStatus { get; set; }
        public ReimbursementStatus ToStatus { get; set; }
        public DateTime Timestamp { get; set; }
    }

    // ---------------------------------------------------------------------
    // Report outputs
    // ---------------------------------------------------------------------

    public class FinancialReportFilter
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public Guid? DepartmentId { get; set; }

        /// <summary>"department", "personal" or null for all.</summary>
        public string? Classification { get; set; }
        public ReimbursementStatus? Status { get; set; }
        public Guid? PayeeUserId { get; set; }
    }

    public class FinanceSummaryDto
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }

        // Flow within the period
        public decimal PaidInPeriod { get; set; }
        public decimal PaidDepartmentInPeriod { get; set; }
        public decimal PaidPersonalUseInPeriod { get; set; }
        public int PaidCountInPeriod { get; set; }
        public decimal AveragePaidInPeriod { get; set; }
        public int SubmittedCountInPeriod { get; set; }
        public decimal RequestedInPeriod { get; set; }
        public int ApprovedCountInPeriod { get; set; }
        public decimal ApprovedInPeriod { get; set; }
        public int RejectedCountInPeriod { get; set; }

        // Current pipeline (as of now, regardless of period)
        public decimal ApprovedNotPaidNow { get; set; }
        public int ApprovedNotPaidCountNow { get; set; }
        public decimal PendingApprovalNow { get; set; }
        public int PendingApprovalCountNow { get; set; }
        public decimal UnderReviewNow { get; set; }
        public int UnderReviewCountNow { get; set; }
        public decimal ReturnedForCorrectionNow { get; set; }
        public int ReturnedForCorrectionCountNow { get; set; }

        // Budgets (calendar year of "To")
        public int BudgetYear { get; set; }
        public int ActiveDepartmentCount { get; set; }
        public decimal TotalActiveAnnualBudget { get; set; }
        public decimal DepartmentPaidThisYear { get; set; }
        public decimal DepartmentCommittedThisYear { get; set; }
        public decimal? BudgetUtilisationPercent { get; set; }
        public int DepartmentsNearLimit { get; set; }
        public int DepartmentsOverBudget { get; set; }
        public decimal WarningThresholdPercent { get; set; }

        public int UnlinkedLegacyRequestCount { get; set; }
        public List<string> Definitions { get; set; } = [];
    }

    public class LabelAmountDto
    {
        public required string Key { get; set; }
        public required string Label { get; set; }
        public bool IsPersonalUse { get; set; }
        public decimal Amount { get; set; }
        public int Count { get; set; }
    }

    public class LargestPaymentDto
    {
        public Guid Id { get; set; }
        public required string RequestNumber { get; set; }
        public required string PayeeName { get; set; }
        public required string Department { get; set; }
        public decimal Amount { get; set; }
        public DateTime? PaymentDate { get; set; }
    }

    public class MonthlyExpenditureDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public required string MonthLabel { get; set; }

        public decimal PaidTotal { get; set; }
        public int PaidCount { get; set; }
        public decimal AveragePaid { get; set; }
        public LargestPaymentDto? LargestPaid { get; set; }

        public decimal ApprovedTotal { get; set; }
        public int ApprovedCount { get; set; }
        public decimal RequestedTotal { get; set; }
        public int RequestedCount { get; set; }

        /// <summary>Approved on or before month end and not paid by month end.</summary>
        public int AwaitingPaymentCount { get; set; }
        public decimal AwaitingPaymentAmount { get; set; }

        public decimal PreviousMonthPaidTotal { get; set; }
        public decimal? PaidChangePercent { get; set; }

        public decimal PaidDepartmentTotal { get; set; }
        public decimal PaidPersonalUseTotal { get; set; }
        public List<LabelAmountDto> PaidByDepartment { get; set; } = [];
        public List<string> Definitions { get; set; } = [];
    }

    public class DepartmentSpendingDto
    {
        public required string Key { get; set; }
        public Guid? DepartmentId { get; set; }
        public required string Department { get; set; }
        public bool IsPersonalUse { get; set; }
        public bool IsLegacyUnlinked { get; set; }
        public bool IsActive { get; set; }

        public decimal? AnnualBudget { get; set; }
        public decimal? MonthlyBudget { get; set; }
        public decimal Paid { get; set; }
        public decimal Committed { get; set; }
        public decimal Pending { get; set; }
        public decimal? Remaining { get; set; }
        public decimal? UtilisationPercent { get; set; }

        public int RequestCount { get; set; }
        public int PaidCount { get; set; }
        public decimal AveragePaid { get; set; }
        public decimal LargestPaid { get; set; }
        public int RejectedCount { get; set; }

        public bool IsHighestPaid { get; set; }
        public bool IsHighestUtilisation { get; set; }
        public bool IsOverBudget { get; set; }
        public bool IsNearLimit { get; set; }
        public bool HasHighPending { get; set; }
    }

    public class DepartmentSpendingReportDto
    {
        public int Year { get; set; }
        public decimal WarningThresholdPercent { get; set; }
        public List<DepartmentSpendingDto> Rows { get; set; } = [];
        public decimal DepartmentPaidTotal { get; set; }
        public decimal PersonalUsePaidTotal { get; set; }
        public decimal PaidTotal { get; set; }
        public List<string> Definitions { get; set; } = [];
    }

    public class TrendPointDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public required string MonthLabel { get; set; }
        public decimal Paid { get; set; }
        public decimal PaidDepartment { get; set; }
        public decimal PaidPersonalUse { get; set; }
        public int PaidCount { get; set; }
        public decimal AveragePaid { get; set; }
        public int SubmittedCount { get; set; }
        public decimal Requested { get; set; }

        /// <summary>Year-to-date (paid + committed) for departments / total active annual budget.</summary>
        public decimal? YearToDateUtilisationPercent { get; set; }
    }

    public class DepartmentTrendSeriesDto
    {
        public required string Key { get; set; }
        public required string Label { get; set; }
        public List<decimal> PaidByMonth { get; set; } = [];
    }

    public class SpendingTrendsDto
    {
        public List<TrendPointDto> Points { get; set; } = [];
        public List<DepartmentTrendSeriesDto> ByDepartment { get; set; } = [];
        public List<string> Definitions { get; set; } = [];
    }

    public class StatusBucketDto
    {
        public ReimbursementStatus Status { get; set; }
        public required string StatusName { get; set; }
        public int Count { get; set; }
        public decimal Amount { get; set; }
    }

    public class AwaitingActionDto
    {
        public Guid Id { get; set; }
        public required string RequestNumber { get; set; }
        public required string PayeeName { get; set; }
        public required string Department { get; set; }
        public ReimbursementStatus Status { get; set; }
        public required string StatusName { get; set; }
        public required string WaitingFor { get; set; }
        public decimal Amount { get; set; }
        public DateTime InStatusSince { get; set; }
        public double DaysInStatus { get; set; }
        public bool IsStale { get; set; }
    }

    public class WorkflowReportDto
    {
        public int StaleThresholdDays { get; set; }
        public List<StatusBucketDto> ByStatus { get; set; } = [];
        public List<AwaitingActionDto> AwaitingAction { get; set; } = [];
        public int StaleCount { get; set; }

        public double? AverageDaysUnderReview { get; set; }
        public int UnderReviewSampleSize { get; set; }
        public double? AverageDaysSubmissionToApproval { get; set; }
        public int SubmissionToApprovalSampleSize { get; set; }
        public double? AverageDaysApprovalToPayment { get; set; }
        public int ApprovalToPaymentSampleSize { get; set; }
        public List<string> Definitions { get; set; } = [];
    }

    public class DepartmentForecastDto
    {
        public required string Key { get; set; }
        public Guid? DepartmentId { get; set; }
        public required string Department { get; set; }
        public decimal? AnnualBudget { get; set; }
        public decimal PaidYearToDate { get; set; }
        public decimal Committed { get; set; }
        public decimal Pending { get; set; }

        /// <summary>Estimate only: (paid + committed) / months elapsed x 12.</summary>
        public decimal? ProjectedYearEnd { get; set; }
        public decimal? ProjectedOverrun { get; set; }
        public string? ProjectedExhaustionMonth { get; set; }

        /// <summary>Remaining budget if every pending request were approved.</summary>
        public decimal? RemainingIfPendingApproved { get; set; }
        public bool WouldOverrunIfPendingApproved { get; set; }

        public decimal LastThreeMonthsPaid { get; set; }
        public decimal PreviousThreeMonthsPaid { get; set; }
        public bool IsIncreasing { get; set; }

        public bool IsReliable { get; set; }
        public string? ReliabilityNote { get; set; }
    }

    public class UnusualReimbursementDto
    {
        public Guid Id { get; set; }
        public required string RequestNumber { get; set; }
        public required string Department { get; set; }
        public required string PayeeName { get; set; }
        public decimal Amount { get; set; }
        public decimal DepartmentAverage { get; set; }
        public required string Reason { get; set; }
    }

    public class FinanceForecastDto
    {
        public int Year { get; set; }
        public int MonthsElapsed { get; set; }
        public List<DepartmentForecastDto> Departments { get; set; } = [];
        public List<UnusualReimbursementDto> UnusualReimbursements { get; set; } = [];
        public decimal OutstandingApprovedCommitments { get; set; }
        public decimal PaidThisMonth { get; set; }
        public decimal PaidSameMonthLastYear { get; set; }
        public decimal PaidPreviousMonth { get; set; }
        public List<string> Definitions { get; set; } = [];
    }
}
