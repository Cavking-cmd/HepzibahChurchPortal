namespace ChurchPortal.Core.Entities
{
    public class AuditLog : BaseEntity
    {
        public required string EntityName { get; set; }
        public Guid EntityId { get; set; }
        public required string Action { get; set; }
        public Guid UserId { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? Details { get; set; }
    }
}
