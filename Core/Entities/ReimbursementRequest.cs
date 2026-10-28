namespace ChurchPortal.Core.Entities
{
    public class ReimbursementRequest : BaseEntity
    {
        public required string RequestNumber { get; set; }
        /// <summary>The member who submitted the request and is owed the money.</summary>
        public Guid PayeeUserId { get; set; }
        public User? PayeeUser { get; set; }
        public required string PayeeName { get; set; }

        /// <summary>
        /// Department name as it read when the request was saved. For requests created
        /// before departments existed this is the original free text, kept for history
        /// until an administrator links it to a department record.
        /// </summary>
        public required string Department { get; set; }

        public ReimbursementClassification Classification { get; set; } = ReimbursementClassification.Department;
        public Guid? DepartmentId { get; set; }
        public Department? DepartmentRecord { get; set; }

        public required string MinisterInCharge { get; set; }
        public DateTime RequestDate { get; set; }
        public string? Remarks { get; set; }

        public decimal Subtotal { get; set; }
        public decimal Gst { get; set; }
        public decimal Pst { get; set; }
        public decimal Hst { get; set; }
        public decimal TotalAmount { get; set; }

        public ReimbursementStatus Status { get; set; } = ReimbursementStatus.Draft;
        public DateTime? SubmittedDate { get; set; }

        // Legacy: the old minister-approval step. Kept for history; no longer written.
        public Guid? MinisterApprovedBy { get; set; }
        public DateTime? MinisterApprovedDate { get; set; }

        // Stage two: Accounts Officer review.
        public Guid? ReviewedBy { get; set; }
        public DateTime? ReviewedDate { get; set; }
        public string? ReviewNotes { get; set; }

        // Stage three: Accounts Admin final approval.
        public Guid? ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public decimal? ApprovedAmount { get; set; }
        public string? ApprovalNotes { get; set; }
        public bool OverBudgetAtApproval { get; set; }
        public string? OverBudgetJustification { get; set; }

        /// <summary>
        /// True when an Accounts Admin approved while physical items were still unverified
        /// or rejected by the Inventory Officer. The reason is required and kept here.
        /// </summary>
        public bool InventoryOverride { get; set; }
        public string? InventoryOverrideReason { get; set; }

        // Payment confirmation (Accounts Admin).
        public Guid? PaidBy { get; set; }
        /// <summary>The date the money was paid, entered when payment is confirmed.</summary>
        public DateTime? PaymentDate { get; set; }
        /// <summary>Server time at which payment was confirmed in the portal.</summary>
        public DateTime? PaymentConfirmedAt { get; set; }
        public decimal? AmountPaid { get; set; }
        public string? PaymentMethod { get; set; }
        public string? PaymentReference { get; set; }

        public string? PreparedBy { get; set; }
        public string? ChequeNumber { get; set; }
        public string? AuthorizedByName1 { get; set; }
        public string? AuthorizedByName2 { get; set; }
        public DateTime? AuthorizationDate { get; set; }
        public string? FinanceNotes { get; set; }

        public Guid? RejectedBy { get; set; }
        public DateTime? RejectedDate { get; set; }
        public string? RejectionReason { get; set; }

        /// <summary>Latest correction request sent back to the member.</summary>
        public string? InfoRequestNotes { get; set; }

        /// <summary>
        /// PostgreSQL xmin row version. Two people acting on the same request at once
        /// cannot both succeed; the second gets a concurrency error.
        /// </summary>
        public uint Version { get; set; }

        public ICollection<ReimbursementItem> Items { get; set; } = new List<ReimbursementItem>();
        public ICollection<ReimbursementReceipt> Receipts { get; set; } = new List<ReimbursementReceipt>();
        public ICollection<ReimbursementHistory> History { get; set; } = new List<ReimbursementHistory>();
    }
}
