using System.ComponentModel.DataAnnotations;

namespace ChurchPortal.Core.Dtos.UserDtos
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public required string Email { get; set; }
        public string? DisplayName { get; set; }
        public string? AvatarBase64 { get; set; }
        public List<string> UserRoles { get; set; } = [];
    }

    public class LoginRequestModel
    {
        [EmailAddress]
        public required string Email { get; set; }
        [Required]
        public required string Password { get; set; }
    }

    public class RegisterRequestModel
    {
        [EmailAddress]
        public required string Email { get; set; }
        [Required]
        public required string Password { get; set; }
        public required List<string> Roles { get; set; }
    }

    public class UpdateProfileRequest
    {
        public string? DisplayName { get; set; }
        public string? AvatarBase64 { get; set; }
    }

    public class UpdateEmailRequest
    {
        [EmailAddress]
        public required string NewEmail { get; set; }
        [Required]
        public required string CurrentPassword { get; set; }
    }

    public class UpdatePasswordRequest
    {
        [Required]
        public required string CurrentPassword { get; set; }
        [Required]
        public required string NewPassword { get; set; }
    }

    public class LoginResultDto
    {
        public required string Message { get; set; }
        public required string Token { get; set; }
        public required UserDto User { get; set; }
    }
}
