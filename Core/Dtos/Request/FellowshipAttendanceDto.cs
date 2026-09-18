namespace ChurchPortal.Core.Dtos.Request
{
    public class FellowshipAttendanceDto
    {
        public Guid Id { get; set; }
        public Guid FellowshipCenterId { get; set; }
        public DateTime Date { get; set; }
        public int Men { get; set; }
        public int Women { get; set; }
        public int Children { get; set; }
        public int NewConverts { get; set; }
        public int Total { get; set; }
        public bool IsApproved { get; set; }
        public bool IsLocked { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public DateTime? ApprovedDate { get; set; }
    }

    public class CreateFellowshipAttendanceRequestModel
    {
        public required Guid FellowshipCenterId { get; set; }
        public DateTime Date { get; set; }
        public int Men { get; set; }
        public int Women { get; set; }
        public int Children { get; set; }
        public int NewConverts { get; set; }
    }

    public class UpdateFellowshipAttendanceModel
    {
        public required Guid Id { get; set; }
        public required Guid FellowshipCenterId { get; set; }
        public DateTime Date { get; set; }
        public int Men { get; set; }
        public int Women { get; set; }
        public int Children { get; set; }
        public int NewConverts { get; set; }
    }
}
