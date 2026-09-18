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
        public string? Preacher { get; set; }
        public int OnlineAttendance { get; set; }
    }

    public class CreateServiceRequestModel
    {
        public DateTime Date { get; set; }
        public required string Day { get; set; }
        public ServiceType ServiceType { get; set; }
        public string? Theme { get; set; }
        public string? ScriptureText { get; set; }
        public string? Preacher { get; set; }
        public int OnlineAttendance { get; set; }
    }

    public class UpdateServiceModel
    {
        public required Guid Id { get; set; }
        public DateTime Date { get; set; }
        public required string Day { get; set; }
        public ServiceType ServiceType { get; set; }
        public string? Theme { get; set; }
        public string? ScriptureText { get; set; }
        public string? Preacher { get; set; }
        public int OnlineAttendance { get; set; }
    }
}
