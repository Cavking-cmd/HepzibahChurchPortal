using ChurchPortal.Core.Dtos;
using ChurchPortal.Core.Dtos.Reports;

namespace ChurchPortal.Services.Interfaces
{
    public interface IInventoryReportService
    {
        Task<BaseResponse<InventoryValuationDto>> GetValuationAsync();
        Task<BaseResponse<List<ReplacementAlertDto>>> GetReplacementAlertsAsync(int years);
        Task<BaseResponse<List<MissingVerificationDto>>> GetMissingVerificationAsync(int months);
        Task<BaseResponse<List<CustodianAccountabilityDto>>> GetCustodianAccountabilityAsync();
        Task<List<ReplacementAlertDto>> ComputeReplacementAlertsAsync(int years);
    }
}
