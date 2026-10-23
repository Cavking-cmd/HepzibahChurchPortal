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
        public required string Department { get; set; }
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

    public class ReimbursementDecisionModel
    {
        public string? Notes { get; set; }
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
        public required string Department { get; set; }
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
        public DateTime? ApprovedDate { get; set; }
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
