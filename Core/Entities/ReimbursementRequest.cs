namespace ChurchPortal.Core.Entities
{
    public class ReimbursementRequest : BaseEntity
    {
        public required string RequestNumber { get; set; }
        public Guid PayeeUserId { get; set; }
        public required string PayeeName { get; set; }
        public required string Department { get; set; }
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

        public Guid? MinisterApprovedBy { get; set; }
        public DateTime? MinisterApprovedDate { get; set; }

        public Guid? ReviewedBy { get; set; }
        public DateTime? ReviewedDate { get; set; }

        public Guid? ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }

        public Guid? PaidBy { get; set; }
        public DateTime? PaymentDate { get; set; }
        public decimal? AmountPaid { get; set; }
        public string? PaymentMethod { get; set; }
        public string? PaymentReference { get; set; }

        public string? PreparedBy { get; set; }
        public string? ChequeNumber { get; set; }
        public string? AuthorizedByName1 { get; set; }
        public string? AuthorizedByName2 { get; set; }
        public DateTime? AuthorizationDate { get; set; }
        public string? FinanceNotes { get; set; }

        public string? RejectionReason { get; set; }
        public string? InfoRequestNotes { get; set; }

        public ICollection<ReimbursementItem> Items { get; set; } = new List<ReimbursementItem>();
        public ICollection<ReimbursementReceipt> Receipts { get; set; } = new List<ReimbursementReceipt>();
        public ICollection<ReimbursementHistory> History { get; set; } = new List<ReimbursementHistory>();
    }
}
