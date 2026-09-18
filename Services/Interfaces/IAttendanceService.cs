using ChurchPortal.Core.Dtos;
using ChurchPortal.Core.Dtos.Request;

namespace ChurchPortal.Services.Interfaces
{
    public interface IAttendanceService
    {
        Task<BaseResponse<AttendanceDto>> CreateAsync(CreateAttendanceRequestModel model, Guid userId);
        Task<BaseResponse<ICollection<AttendanceDto>>> GetAll();
        Task<BaseResponse<AttendanceDto>> GetAsync(Guid id);
        Task<BaseResponse<AttendanceDto>> UpdateAsync(UpdateAttendanceModel model, Guid userId);
        Task<BaseResponse<bool>> SoftDeleteAsync(Guid id, Guid userId);
        Task<BaseResponse<AttendanceDto>> ApproveAsync(Guid id, Guid approverUserId);
        Task<BaseResponse<AttendanceDto>> LockAsync(Guid id, Guid userId);
    }
}
