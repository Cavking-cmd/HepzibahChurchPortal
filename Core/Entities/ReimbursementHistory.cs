namespace ChurchPortal.Core.Entities
{
    public class ReimbursementHistory : BaseEntity
    {
        public Guid ReimbursementRequestId { get; set; }
        public ReimbursementRequest? ReimbursementRequest { get; set; }

        public required string Action { get; set; }
        public Guid ActorUserId { get; set; }
        public string? ActorName { get; set; }
        public ReimbursementStatus? FromStatus { get; set; }
        public ReimbursementStatus? ToStatus { get; set; }
        public string? Notes { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
