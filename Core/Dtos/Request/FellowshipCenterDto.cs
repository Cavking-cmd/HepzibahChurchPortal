namespace ChurchPortal.Core.Dtos.Request
{
    public class FellowshipCenterDto
    {
        public Guid Id { get; set; }
        public required string CenterName { get; set; }
        public required string Zone { get; set; }
        public required string LeaderName { get; set; }
        public required string Location { get; set; }
    }

    public class CreateFellowshipCenterRequestModel
    {
        public required string CenterName { get; set; }
        public required string Zone { get; set; }
        public required string LeaderName { get; set; }
        public required string Location { get; set; }
    }

    public class UpdateFellowshipCenterModel
    {
        public required Guid Id { get; set; }
        public required string CenterName { get; set; }
        public required string Zone { get; set; }
        public required string LeaderName { get; set; }
        public required string Location { get; set; }
    }
}
