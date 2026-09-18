namespace ChurchPortal.Core.Dtos.Reports
{
    public class DashboardAlertDto
    {
        public string Type { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class DashboardSummaryDto
    {
        public int TotalAttendanceThisMonth { get; set; }
        public double? GrowthVsLastMonthPercent { get; set; }
        public int TotalActiveFellowshipCenters { get; set; }
        public decimal TotalAssetValue { get; set; }
        public List<DashboardAlertDto> Alerts { get; set; } = new();
    }
}
