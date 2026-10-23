using ChurchPortal.Core.Dtos;
using ChurchPortal.Core.Dtos.UserDtos;

namespace ChurchPortal.Services.Interfaces
{
    public interface IUserService
    {
        Task<BaseResponse<UserDto>> RegisterAsync(RegisterRequestModel model);
        Task<BaseResponse<UserDto>> LoginAsync(LoginRequestModel model);
        Task<bool> DeleteAsync(Guid id);
        Task<BaseResponse<List<UserDto>>> GetAllAsync();
        Task<BaseResponse<UserDto>> GetMeAsync(Guid userId);
        Task<BaseResponse<UserDto>> UpdateProfileAsync(Guid userId, UpdateProfileRequest request);
        Task<BaseResponse<UserDto>> UpdateEmailAsync(Guid userId, UpdateEmailRequest request);
        Task<BaseResponse<bool>> UpdatePasswordAsync(Guid userId, UpdatePasswordRequest request);
        Task<BaseResponse<bool>> ForgotPasswordAsync(ForgotPasswordRequest request);
        Task<BaseResponse<bool>> ResetPasswordAsync(ResetPasswordRequest request);
        Task<BaseResponse<bool>> VerifyEmailAsync(VerifyEmailRequest request);
        Task<BaseResponse<bool>> ResendVerificationAsync(ResendVerificationRequest request);
    }
}
