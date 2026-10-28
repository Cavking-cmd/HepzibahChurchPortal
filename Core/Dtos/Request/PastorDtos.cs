namespace ChurchPortal.Core.Dtos.Request
{
    public class PastorDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int ServiceCount { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

    public class CreatePastorModel
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Description { get; set; }

        /// <summary>
        /// Set to true after the user has seen the possible-duplicate warning and
        /// confirmed this is a different person.
        /// </summary>
        public bool ConfirmNotDuplicate { get; set; }
    }

    public class UpdatePastorModel : CreatePastorModel
    {
        public Guid Id { get; set; }
        public bool IsActive { get; set; } = true;
    }

    /// <summary>Returned with HTTP 409 when a new pastor looks like an existing one.</summary>
    public class PastorDuplicateDto
    {
        public required string Message { get; set; }
        public List<PastorDto> Matches { get; set; } = [];
    }

    /// <summary>Typed preacher names on older services that are not linked to a pastor yet.</summary>
    public class LegacyPreacherDto
    {
        public required string PreacherName { get; set; }
        public int ServiceCount { get; set; }
        public DateTime FirstServiceDate { get; set; }
        public DateTime LastServiceDate { get; set; }
        public List<PastorDto> SuggestedPastors { get; set; } = [];
    }

    public class LinkLegacyPreacherModel
    {
        public string? PreacherName { get; set; }
        public Guid PastorId { get; set; }
    }
}
