namespace ChurchPortal.Core.Entities
{
    public class User : BaseEntity
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
        public string? DisplayName { get; set; }
        public byte[]? AvatarFile { get; set; }
        public string? AvatarMimeType { get; set; }

        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public ICollection<UserCredential> Credentials { get; set; } = new List<UserCredential>();
    }
}
