using ChurchPortal.Core.Dtos;
using ChurchPortal.Core.Dtos.Reports;

namespace ChurchPortal.Services.Interfaces
{
    public interface IAttendanceReportService
    {
        Task<BaseResponse<List<MonthlyGrowthDto>>> GetMonthlyGrowthAsync(int months);
        Task<BaseResponse<ServiceComparisonReportDto>> GetServiceComparisonAsync(DateTime? month = null);
        Task<BaseResponse<MonthServiceAttendanceDto>> GetMonthByServiceAsync(DateTime month);
        Task<BaseResponse<DemographicsReportDto>> GetDemographicsAsync(DateTime? startDate, DateTime? endDate);
        Task<BaseResponse<FirstTimerConversionDto>> GetFirstTimerConversionAsync(DateTime? month = null);
        Task<BaseResponse<List<PreacherImpactDto>>> GetPreacherImpactAsync(DateTime? month = null);
        Task<BaseResponse<List<MonthOptionDto>>> GetAvailableMonthsAsync();
    }
}
