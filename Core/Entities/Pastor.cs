namespace ChurchPortal.Core.Entities
{
    /// <summary>
    /// A pastor or guest minister as a person, used when recording services.
    /// This is a name record, not a login: it has nothing to do with user roles.
    /// </summary>
    public class Pastor : BaseEntity
    {
        public required string Name { get; set; }

        /// <summary>Normalised form of the name used for duplicate warnings.</summary>
        public required string NormalizedName { get; set; }

        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }

        /// <summary>Free text, for example "Home pastor" or "Guest pastor".</summary>
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime? UpdatedDate { get; set; }

        public ICollection<Service> Services { get; set; } = new List<Service>();
    }
}
