using ChurchPortal.Core.Dtos;
using ChurchPortal.Core.Dtos.Reports;

namespace ChurchPortal.Services.Interfaces
{
    public interface IFellowshipReportService
    {
        Task<BaseResponse<List<CenterRankingDto>>> GetCenterRankingAsync();
        Task<BaseResponse<List<ZoneSummaryDto>>> GetZoneSummaryAsync();
        Task<BaseResponse<List<LeaderTrendDto>>> GetLeaderTrendAsync();
        Task<BaseResponse<List<ExpansionAlertDto>>> GetExpansionAlertsAsync();
        Task<List<ExpansionAlertDto>> ComputeExpansionAlertsAsync();
    }
}
