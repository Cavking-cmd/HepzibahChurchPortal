using ChurchPortal.Core.Entities;

namespace ChurchPortal.Core.Dtos.Request
{
    public class ServiceDto
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public required string Day { get; set; }
        public ServiceType ServiceType { get; set; }
        public string? Theme { get; set; }
        public string? ScriptureText { get; set; }

        /// <summary>Selected pastor. Null for older services still holding typed text.</summary>
        public Guid? PastorId { get; set; }

        /// <summary>Canonical pastor name, or the original typed text on older services.</summary>
        public string? Preacher { get; set; }

        /// <summary>True when Preacher is old free text that has not been linked to a pastor record.</summary>
        public bool PreacherNeedsLink { get; set; }
    }

    public class CreateServiceRequestModel
    {
        public DateTime Date { get; set; }
        public required string Day { get; set; }
        public ServiceType ServiceType { get; set; }
        public string? Theme { get; set; }
        public string? ScriptureText { get; set; }
        public Guid? PastorId { get; set; }

        /// <summary>
        /// Ignored for new services. On edit, sending back the existing unlinked text
        /// unchanged keeps it; anything else is dropped. Pick a pastor instead.
        /// </summary>
        public string? Preacher { get; set; }
    }

    public class UpdateServiceModel : CreateServiceRequestModel
    {
        public required Guid Id { get; set; }
    }
}
