namespace ChurchPortal.Core.Entities
{
    public class FellowshipCenter : BaseEntity
    {
        public required string CenterName { get; set; }
        public required string Zone { get; set; }
        public required string LeaderName { get; set; }
        public required string Location { get; set; }

        public ICollection<FellowshipAttendance> FellowshipAttendances { get; set; } = new List<FellowshipAttendance>();
    }
}
