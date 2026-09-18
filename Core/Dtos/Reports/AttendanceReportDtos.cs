namespace ChurchPortal.Core.Dtos.Reports
{
    public class MonthlyGrowthDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthLabel { get; set; } = string.Empty;
        public int TotalAttendance { get; set; }
        public double? PercentChangeFromPreviousMonth { get; set; }
    }

    public class ServiceTypeComparisonDto
    {
        public string ServiceType { get; set; } = string.Empty;
        public int TotalAttendance { get; set; }
        public double AverageAttendance { get; set; }
        public int RecordCount { get; set; }
    }

    public class OnlineVsPhysicalDto
    {
        public string MonthLabel { get; set; } = string.Empty;
        public int TotalOnline { get; set; }
        public int TotalPhysical { get; set; }
    }

    public class ServiceComparisonReportDto
    {
        public List<ServiceTypeComparisonDto> ByServiceType { get; set; } = new();
        public List<OnlineVsPhysicalDto> OnlineVsPhysicalByMonth { get; set; } = new();
    }

    public class DemographicsReportDto
    {
        public int TotalMen { get; set; }
        public int TotalWomen { get; set; }
        public int TotalChildren { get; set; }
    }

    public class FirstTimerConversionDto
    {
        public int TotalFirstTimers { get; set; }
        public int TotalNewConverts { get; set; }
        public double ConversionRatePercent { get; set; }
    }

    public class PreacherImpactDto
    {
        public string Preacher { get; set; } = string.Empty;
        public int TotalAttendance { get; set; }
        public double AverageAttendance { get; set; }
        public int ServiceCount { get; set; }
    }
}
