namespace ChurchPortal.Core.Entities
{
    public class Role : BaseEntity
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
