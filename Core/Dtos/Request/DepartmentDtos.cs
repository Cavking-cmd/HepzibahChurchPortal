namespace ChurchPortal.Core.Dtos.Request
{
    /// <summary>Minimal shape for the reimbursement form dropdown.</summary>
    public class DepartmentOptionDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
    }

    public class DepartmentDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public decimal AnnualBudget { get; set; }
        public decimal? MonthlyBudget { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int RequestCount { get; set; }
    }

    public class CreateDepartmentModel
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? AnnualBudget { get; set; }
        public decimal? MonthlyBudget { get; set; }
    }

    public class UpdateDepartmentModel : CreateDepartmentModel
    {
        public Guid Id { get; set; }
        public bool IsActive { get; set; } = true;
    }

    /// <summary>Free-text department names on old requests that are not linked yet.</summary>
    public class LegacyDepartmentNameDto
    {
        public required string LegacyName { get; set; }
        public int RequestCount { get; set; }
        public decimal TotalAmount { get; set; }
        public Guid? SuggestedDepartmentId { get; set; }
        public string? SuggestedDepartmentName { get; set; }
    }

    public class LinkLegacyDepartmentModel
    {
        public string? LegacyName { get; set; }

        /// <summary>Department to link to. Leave empty and set PersonalUse to classify as personal use.</summary>
        public Guid? DepartmentId { get; set; }
        public bool PersonalUse { get; set; }
    }

    public class BudgetAlertDto
    {
        /// <summary>WouldExceedAnnual, WouldExceedMonthly, NearAnnualLimit, OverAnnual, OverMonthly, HighPending.</summary>
        public required string Code { get; set; }

        /// <summary>info, warning or danger.</summary>
        public required string Severity { get; set; }
        public required string Message { get; set; }
    }

    /// <summary>
    /// Budget position for one department, worked out on the server.
    /// Amounts are counted in the period of the event that put them in their bucket:
    ///   Paid        = amount paid, by payment date
    ///   Committed   = approved but not paid yet, by approval date
    ///   Pending     = under review or pending approval, by submission date (shown separately, not spent)
    ///   Remaining   = budget - paid - committed (pending is NOT subtracted)
    ///   Utilisation = (paid + committed) / budget
    /// A request is only ever in one bucket, so nothing is counted twice.
    /// </summary>
    public class DepartmentBudgetDto
    {
        public Guid DepartmentId { get; set; }
        public required string DepartmentName { get; set; }
        public bool IsActive { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }

        public decimal AnnualBudget { get; set; }
        public decimal PaidThisYear { get; set; }
        public decimal CommittedThisYear { get; set; }
        public decimal PendingThisYear { get; set; }
        public decimal RemainingThisYear { get; set; }
        public decimal? UtilisationPercentThisYear { get; set; }

        public decimal? MonthlyBudget { get; set; }
        public decimal PaidThisMonth { get; set; }
        public decimal CommittedThisMonth { get; set; }
        public decimal PendingThisMonth { get; set; }
        public decimal? RemainingThisMonth { get; set; }
        public decimal? UtilisationPercentThisMonth { get; set; }

        public List<BudgetAlertDto> Alerts { get; set; } = [];
    }

    /// <summary>Budget picture for one reimbursement under review or approval.</summary>
    public class ReimbursementBudgetDto
    {
        public Guid ReimbursementId { get; set; }
        public bool IsPersonalUse { get; set; }
        public bool NeedsDepartmentLink { get; set; }
        public string? Note { get; set; }

        /// <summary>Amount that would be committed if approved (approved amount, else total).</summary>
        public decimal RequestAmount { get; set; }

        /// <summary>True when this request is already counted inside the figures below.</summary>
        public bool AlreadyCommitted { get; set; }

        public DepartmentBudgetDto? Department { get; set; }

        public decimal? ProjectedRemainingThisYear { get; set; }
        public decimal? ProjectedUtilisationPercentThisYear { get; set; }
        public decimal? ProjectedRemainingThisMonth { get; set; }
        public decimal? ProjectedUtilisationPercentThisMonth { get; set; }
        public bool WouldExceedAnnual { get; set; }
        public bool WouldExceedMonthly { get; set; }
        public bool RequiresOverBudgetJustification { get; set; }

        public List<BudgetAlertDto> Alerts { get; set; } = [];
        public string CalculationNote { get; set; } = string.Empty;
    }
}
