namespace ChurchPortal.Core.Entities
{
    public class Department : BaseEntity
    {
        public required string Name { get; set; }

        /// <summary>Upper-cased, trimmed, single-spaced name used for the unique index.</summary>
        public required string NormalizedName { get; set; }

        public string? Description { get; set; }

        /// <summary>Budget for a calendar year (January to December). Required.</summary>
        public decimal AnnualBudget { get; set; }

        /// <summary>Optional monthly spending guide. Null means no monthly limit is set.</summary>
        public decimal? MonthlyBudget { get; set; }

        /// <summary>Inactive departments keep their history but cannot receive new requests.</summary>
        public bool IsActive { get; set; } = true;

        public DateTime? UpdatedDate { get; set; }

        public ICollection<ReimbursementRequest> Reimbursements { get; set; } = new List<ReimbursementRequest>();
    }
}
