namespace ChurchPortal.Core.Dtos.Reports
{
    public class CenterRankingDto
    {
        public Guid CenterId { get; set; }
        public string CenterName { get; set; } = string.Empty;
        public string Zone { get; set; } = string.Empty;
        public int TotalAttendance { get; set; }
        public double AverageAttendance { get; set; }
        public int RecordCount { get; set; }
    }

    public class ZoneSummaryDto
    {
        public string Zone { get; set; } = string.Empty;
        public int TotalAttendance { get; set; }
        public int CenterCount { get; set; }
        public double AverageAttendance { get; set; }
    }

    public class LeaderTrendDto
    {
        public string LeaderName { get; set; } = string.Empty;
        public string CenterName { get; set; } = string.Empty;
        public double AverageAttendance { get; set; }
        public int RecordCount { get; set; }
    }

    public class ExpansionAlertDto
    {
        public Guid CenterId { get; set; }
        public string CenterName { get; set; } = string.Empty;
        public string AlertType { get; set; } = "Declining";
        public string Detail { get; set; } = string.Empty;
    }
}
