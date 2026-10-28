namespace ChurchPortal.Core.Entities
{
    public enum ServiceType
    {
        SundayService,
        WednesdayBibleStudy,
        Rodah,
        Special
    }

    public class Service : BaseEntity
    {
        public DateTime Date { get; set; }
        public required string Day { get; set; }
        public ServiceType ServiceType { get; set; }
        public string? Theme { get; set; }
        public string? ScriptureText { get; set; }

        /// <summary>Selected pastor record. Null for older services not yet linked.</summary>
        public Guid? PastorId { get; set; }
        public Pastor? Pastor { get; set; }

        /// <summary>
        /// Preacher name. When PastorId is set this is the pastor's canonical name,
        /// kept in sync on rename. For older services it is the original typed text.
        /// </summary>
        public string? Preacher { get; set; }

        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    }
}
