using ChurchPortal.Core.Dtos;
using ChurchPortal.Core.Dtos.Request;

namespace ChurchPortal.Services.Interfaces
{
    public interface IServiceService
    {
        Task<BaseResponse<ServiceDto>> CreateAsync(CreateServiceRequestModel model, Guid userId);
        Task<BaseResponse<ICollection<ServiceDto>>> GetAll();
        Task<BaseResponse<ServiceDto>> GetAsync(Guid id);
        Task<BaseResponse<ServiceDto>> UpdateAsync(UpdateServiceModel model, Guid userId);
        Task<BaseResponse<bool>> SoftDeleteAsync(Guid id, Guid userId);
    }
}
