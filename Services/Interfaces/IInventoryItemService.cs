using ChurchPortal.Core.Dtos;
using ChurchPortal.Core.Dtos.Request;

namespace ChurchPortal.Services.Interfaces
{
    public interface IInventoryItemService
    {
        Task<BaseResponse<InventoryItemDto>> CreateAsync(CreateInventoryItemRequestModel model, Guid userId);
        Task<BaseResponse<ICollection<InventoryItemDto>>> GetAll();
        Task<BaseResponse<InventoryItemDto>> GetAsync(Guid id);
        Task<BaseResponse<InventoryItemDto>> UpdateAsync(UpdateInventoryItemModel model, Guid userId);
        Task<BaseResponse<bool>> SoftDeleteAsync(Guid id, Guid userId);
    }
}
