using ChurchPortal.Core.Dtos;
using ChurchPortal.Core.Dtos.Request;

namespace ChurchPortal.Services.Interfaces
{
    public interface IFellowshipCenterService
    {
        Task<BaseResponse<FellowshipCenterDto>> CreateAsync(CreateFellowshipCenterRequestModel model, Guid userId);
        Task<BaseResponse<ICollection<FellowshipCenterDto>>> GetAll();
        Task<BaseResponse<FellowshipCenterDto>> GetAsync(Guid id);
        Task<BaseResponse<FellowshipCenterDto>> UpdateAsync(UpdateFellowshipCenterModel model, Guid userId);
        Task<BaseResponse<bool>> SoftDeleteAsync(Guid id, Guid userId);
    }
}
