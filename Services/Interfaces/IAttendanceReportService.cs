using ChurchPortal.Core.Dtos;
using ChurchPortal.Core.Dtos.Reports;

namespace ChurchPortal.Services.Interfaces
{
    public interface IAttendanceReportService
    {
        Task<BaseResponse<List<MonthlyGrowthDto>>> GetMonthlyGrowthAsync(int months);
        Task<BaseResponse<ServiceComparisonReportDto>> GetServiceComparisonAsync();
        Task<BaseResponse<DemographicsReportDto>> GetDemographicsAsync(DateTime? startDate, DateTime? endDate);
        Task<BaseResponse<FirstTimerConversionDto>> GetFirstTimerConversionAsync();
        Task<BaseResponse<List<PreacherImpactDto>>> GetPreacherImpactAsync();
    }
}
