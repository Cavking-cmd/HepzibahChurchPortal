using ChurchPortal.Core.Dtos.UserDtos;

namespace ChurchPortal.AuthService
{
    public interface IAuthService
    {
        string GenerateToken(UserDto userDto);
    }
}
