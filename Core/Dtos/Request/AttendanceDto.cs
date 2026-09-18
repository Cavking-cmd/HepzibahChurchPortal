namespace ChurchPortal.Core.Dtos.Request
{
    public class AttendanceDto
    {
        public Guid Id { get; set; }
        public Guid ServiceId { get; set; }
        public int Men { get; set; }
        public int Women { get; set; }
        public int Children { get; set; }
        public int SundaySchool { get; set; }
        public int NewConverts { get; set; }
        public int FirstTimers { get; set; }
        public int Total { get; set; }
        public bool IsApproved { get; set; }
        public bool IsLocked { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public DateTime? ApprovedDate { get; set; }
    }

    public class CreateAttendanceRequestModel
    {
        public required Guid ServiceId { get; set; }
        public int Men { get; set; }
        public int Women { get; set; }
        public int Children { get; set; }
        public int SundaySchool { get; set; }
        public int NewConverts { get; set; }
        public int FirstTimers { get; set; }
    }

    public class UpdateAttendanceModel
    {
        public required Guid Id { get; set; }
        public required Guid ServiceId { get; set; }
        public int Men { get; set; }
        public int Women { get; set; }
        public int Children { get; set; }
        public int SundaySchool { get; set; }
        public int NewConverts { get; set; }
        public int FirstTimers { get; set; }
    }
}
