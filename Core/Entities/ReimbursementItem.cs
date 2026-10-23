namespace ChurchPortal.Core.Entities
{
    public class ReimbursementItem : BaseEntity
    {
        public Guid ReimbursementRequestId { get; set; }
        public ReimbursementRequest? ReimbursementRequest { get; set; }

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
}
