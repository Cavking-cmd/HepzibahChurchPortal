using ChurchPortal.Core.Dtos;
using ChurchPortal.Core.Dtos.UserDtos;

namespace ChurchPortal.Services.Interfaces
{
    public interface IUserService
    {
        Task<BaseResponse<UserDto>> RegisterAsync(RegisterRequestModel model);
        Task<BaseResponse<UserDto>> LoginAsync(LoginRequestModel model);
        Task<bool> DeleteAsync(Guid id);
        Task<BaseResponse<UserDto>> GetMeAsync(Guid userId);
        Task<BaseResponse<UserDto>> UpdateProfileAsync(Guid userId, UpdateProfileRequest request);
        Task<BaseResponse<UserDto>> UpdateEmailAsync(Guid userId, UpdateEmailRequest request);
        Task<BaseResponse<bool>> UpdatePasswordAsync(Guid userId, UpdatePasswordRequest request);
    }
}
