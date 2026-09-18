using ChurchPortal.Core.Dtos;
using ChurchPortal.Core.Dtos.Reports;

namespace ChurchPortal.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<BaseResponse<DashboardSummaryDto>> GetSummaryAsync();
    }
}
