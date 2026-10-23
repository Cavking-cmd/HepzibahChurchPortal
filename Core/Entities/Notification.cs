namespace ChurchPortal.Core.Entities
{
    public class Notification : BaseEntity
    {
        public Guid UserId { get; set; }
        public required string Title { get; set; }
        public required string Message { get; set; }
        public string? Link { get; set; }
        public Guid? ReimbursementRequestId { get; set; }
        public bool IsRead { get; set; }
    }
}
