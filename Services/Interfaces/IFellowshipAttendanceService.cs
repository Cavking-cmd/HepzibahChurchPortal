using ChurchPortal.Core.Dtos;
using ChurchPortal.Core.Dtos.Request;

namespace ChurchPortal.Services.Interfaces
{
    public interface IFellowshipAttendanceService
    {
        Task<BaseResponse<FellowshipAttendanceDto>> CreateAsync(CreateFellowshipAttendanceRequestModel model, Guid userId);
        Task<BaseResponse<ICollection<FellowshipAttendanceDto>>> GetAll();
        Task<BaseResponse<FellowshipAttendanceDto>> GetAsync(Guid id);
        Task<BaseResponse<FellowshipAttendanceDto>> UpdateAsync(UpdateFellowshipAttendanceModel model, Guid userId);
        Task<BaseResponse<bool>> SoftDeleteAsync(Guid id, Guid userId);
        Task<BaseResponse<FellowshipAttendanceDto>> ApproveAsync(Guid id, Guid approverUserId);
        Task<BaseResponse<FellowshipAttendanceDto>> LockAsync(Guid id, Guid userId);
    }
}
