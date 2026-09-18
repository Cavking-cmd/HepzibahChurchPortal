namespace ChurchPortal.Core.Entities
{
    public enum ServiceType
    {
        Sunday,
        Wednesday,
        Special
    }

    public class Service : BaseEntity
    {
        public DateTime Date { get; set; }
        public required string Day { get; set; }
        public ServiceType ServiceType { get; set; }
        public string? Theme { get; set; }
        public string? ScriptureText { get; set; }
        public string? Preacher { get; set; }
        public int OnlineAttendance { get; set; }

        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    }
}
