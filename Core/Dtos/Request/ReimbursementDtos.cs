using ChurchPortal.Core.Entities;

namespace ChurchPortal.Core.Dtos.Request
{
    public class ReimbursementItemRequestModel
    {
        public int ReferenceNumber { get; set; }
        public required string ItemName { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Gst { get; set; }
        public decimal Pst { get; set; }
        public decimal Hst { get; set; }
        public string? SerialNumber { get; set; }
    }

    public class ReimbursementReceiptUploadModel
    {
        public required string FileName { get; set; }
        public required string ContentType { get; set; }
        public required string DataBase64 { get; set; }
    }

    public class CreateReimbursementRequestModel
    {
        public required string PayeeName { get; set; }

        /// <summary>Department or PersonalUse. Department requires DepartmentId.</summary>
        public ReimbursementClassification Classification { get; set; } = ReimbursementClassification.Department;
        public Guid? DepartmentId { get; set; }

        public required string MinisterInCharge { get; set; }
        public DateTime RequestDate { get; set; }
        public string? Remarks { get; set; }
        public List<ReimbursementItemRequestModel> Items { get; set; } = [];
        public List<ReimbursementReceiptUploadModel> Receipts { get; set; } = [];
    }

    public class UpdateReimbursementRequestModel : CreateReimbursementRequestModel
    {
        public required Guid Id { get; set; }
    }

    /// <summary>Accounts Officer forwards a request for final approval.</summary>
    public class ReviewReimbursementModel
    {
        public string? Notes { get; set; }
    }

    /// <summary>Accounts Admin final approval.</summary>
    public class ReimbursementDecisionModel
    {
        public string? Notes { get; set; }

        /// <summary>Defaults to the requested total. Cannot exceed it.</summary>
        public decimal? ApprovedAmount { get; set; }

        /// <summary>Required when the server finds the request would exceed a budget.</summary>
        public string? OverBudgetJustification { get; set; }

        public string? PreparedBy { get; set; }
        public string? ChequeNumber { get; set; }
        public string? AuthorizedByName1 { get; set; }
        public string? AuthorizedByName2 { get; set; }
        public DateTime? AuthorizationDate { get; set; }
    }

    public class RejectReimbursementModel
    {
        public required string Reason { get; set; }
    }

    public class RequestReimbursementInfoModel
    {
        public required string Notes { get; set; }
    }

    public class RecordReimbursementPaymentModel
    {
        public DateTime PaymentDate { get; set; }
        public decimal AmountPaid { get; set; }
        public required string PaymentMethod { get; set; }
        public string? PaymentReference { get; set; }
        public string? PreparedBy { get; set; }
        public string? ChequeNumber { get; set; }
        public string? AuthorizedByName1 { get; set; }
        public string? AuthorizedByName2 { get; set; }
        public DateTime? AuthorizationDate { get; set; }
        public string? FinanceNotes { get; set; }
    }

    public class ReimbursementItemDto
    {
        public Guid Id { get; set; }
        public int ReferenceNumber { get; set; }
        public required string ItemName { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Gst { get; set; }
        public decimal Pst { get; set; }
        public decimal Hst { get; set; }
        public decimal Total { get; set; }
        public bool RequiresSerialNumber { get; set; }
        public string? SerialNumber { get; set; }
        public string? ScreeningNote { get; set; }
    }

    public class ReimbursementReceiptDto
    {
        public Guid Id { get; set; }
        public required string FileName { get; set; }
        public required string ContentType { get; set; }
        public long FileSize { get; set; }
        public Guid UploadedBy { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class ReimbursementHistoryDto
    {
        public Guid Id { get; set; }
        public required string Action { get; set; }
        public Guid ActorUserId { get; set; }
        public string? ActorName { get; set; }
        public ReimbursementStatus? FromStatus { get; set; }
        public ReimbursementStatus? ToStatus { get; set; }
        public string? Notes { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class ReimbursementRequestDto
    {
        public Guid Id { get; set; }
        public required string RequestNumber { get; set; }
        public Guid PayeeUserId { get; set; }
        public string? PayeeEmail { get; set; }
        public required string PayeeName { get; set; }

        /// <summary>Display name: the department record's name, "Personal Use", or the legacy text.</summary>
        public required string Department { get; set; }
        public ReimbursementClassification Classification { get; set; }
        public Guid? DepartmentId { get; set; }

        /// <summary>True when an old request still has only free-text department and needs linking.</summary>
        public bool NeedsDepartmentLink { get; set; }

        public required string MinisterInCharge { get; set; }
        public DateTime RequestDate { get; set; }
        public string? Remarks { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Gst { get; set; }
        public decimal Pst { get; set; }
        public decimal Hst { get; set; }
        public decimal TotalAmount { get; set; }
        public ReimbursementStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public int ReceiptCount { get; set; }
        public DateTime? SubmittedDate { get; set; }
        public DateTime? MinisterApprovedDate { get; set; }

        public DateTime? ReviewedDate { get; set; }
        public string? ReviewedByName { get; set; }
        public string? ReviewNotes { get; set; }

        public DateTime? ApprovedDate { get; set; }
        public string? ApprovedByName { get; set; }
        public decimal? ApprovedAmount { get; set; }
        public string? ApprovalNotes { get; set; }
        public bool OverBudgetAtApproval { get; set; }
        public string? OverBudgetJustification { get; set; }

        public DateTime? PaymentDate { get; set; }
        public DateTime? PaymentConfirmedAt { get; set; }
        public string? PaidByName { get; set; }
        public decimal? AmountPaid { get; set; }
        public string? PaymentMethod { get; set; }
        public string? PaymentReference { get; set; }
        public string? PreparedBy { get; set; }
        public string? ChequeNumber { get; set; }
        public string? AuthorizedByName1 { get; set; }
        public string? AuthorizedByName2 { get; set; }
        public DateTime? AuthorizationDate { get; set; }
        public string? FinanceNotes { get; set; }

        public DateTime? RejectedDate { get; set; }
        public string? RejectedByName { get; set; }
        public string? RejectionReason { get; set; }
        public string? InfoRequestNotes { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<ReimbursementItemDto> Items { get; set; } = [];
        public List<ReimbursementReceiptDto> Receipts { get; set; } = [];
        public List<ReimbursementHistoryDto> History { get; set; } = [];
    }

    public class NotificationDto
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }
        public required string Message { get; set; }
        public string? Link { get; set; }
        public Guid? ReimbursementRequestId { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class ReceiptFileDto
    {
        public required string FileName { get; set; }
        public required string ContentType { get; set; }
        public required byte[] Data { get; set; }
    }
}
