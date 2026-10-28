using ChurchPortal.Core.Dtos;
using ChurchPortal.Core.Dtos.Request;

namespace ChurchPortal.Services.Interfaces
{
    public class PastorSaveResult
    {
        public required BaseResponse<PastorDto> Response { get; set; }

        /// <summary>Set when the save was held back because the name matches existing pastors.</summary>
        public PastorDuplicateDto? Duplicate { get; set; }
    }

    public interface IPastorService
    {
        Task<BaseResponse<List<PastorDto>>> GetAllAsync(bool includeInactive, string? search);
        Task<BaseResponse<List<PastorDto>>> FindPossibleDuplicatesAsync(string? name, Guid? excludeId);
        Task<PastorSaveResult> CreateAsync(CreatePastorModel model, Guid userId);
        Task<PastorSaveResult> UpdateAsync(UpdatePastorModel model, Guid userId);
        Task<BaseResponse<List<LegacyPreacherDto>>> GetLegacyPreachersAsync();
        Task<BaseResponse<int>> LinkLegacyPreacherAsync(LinkLegacyPreacherModel model, Guid userId);
    }
}
