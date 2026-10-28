using ChurchPortal.Core.Dtos;
using ChurchPortal.Core.Dtos.Request;

namespace ChurchPortal.Services.Interfaces
{
    public interface IDepartmentService
    {
        Task<BaseResponse<List<DepartmentOptionDto>>> GetActiveOptionsAsync();
        Task<BaseResponse<List<DepartmentDto>>> GetAllAsync(bool includeInactive);
        Task<BaseResponse<List<DepartmentBudgetDto>>> GetBudgetsAsync(int? year, int? month, bool includeInactive);
        Task<BaseResponse<DepartmentBudgetDto>> GetBudgetAsync(Guid id, int? year, int? month);
        Task<BaseResponse<DepartmentDto>> CreateAsync(CreateDepartmentModel model, Guid userId);
        Task<BaseResponse<DepartmentDto>> UpdateAsync(UpdateDepartmentModel model, Guid userId);
        Task<BaseResponse<List<LegacyDepartmentNameDto>>> GetLegacyNamesAsync();
        Task<BaseResponse<int>> LinkLegacyAsync(LinkLegacyDepartmentModel model, Guid userId);
    }
}
