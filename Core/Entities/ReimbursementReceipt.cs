namespace ChurchPortal.Core.Entities
{
    public class ReimbursementReceipt : BaseEntity
    {
        public Guid ReimbursementRequestId { get; set; }
        public ReimbursementRequest? ReimbursementRequest { get; set; }

        public required string FileName { get; set; }
        public required string ContentType { get; set; }
        public long FileSize { get; set; }
        public required byte[] FileData { get; set; }
        public Guid UploadedBy { get; set; }
    }
}
