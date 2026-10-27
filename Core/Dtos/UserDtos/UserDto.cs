using System.ComponentModel.DataAnnotations;

namespace ChurchPortal.Core.Dtos.UserDtos
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public required string Email { get; set; }
        public string? DisplayName { get; set; }
        public string? AvatarBase64 { get; set; }
        public bool EmailVerified { get; set; }
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

    public class ForgotPasswordRequest
    {
        [EmailAddress]
        public required string Email { get; set; }
    }

    public class ResetPasswordRequest
    {
        [Required]
        public required string Token { get; set; }
        [Required]
        public required string NewPassword { get; set; }
    }

    public class VerifyEmailRequest
    {
        [Required]
        public required string Token { get; set; }
    }

    public class ResendVerificationRequest
    {
        /// <summary>The address the user typed to confirm; must match the account email.</summary>
        public string? ConfirmEmail { get; set; }
    }

    public class VerificationEmailSentDto
    {
        public required string Email { get; set; }
        public DateTime SentAtUtc { get; set; }
    }

    public class EmailStatusDto
    {
        public bool Configured { get; set; }
        public string Provider { get; set; } = "Resend";
        public string? From { get; set; }
        public string? FromName { get; set; }
        public string? FrontendBaseUrl { get; set; }
    }

    public class LoginResultDto
    {
        public required string Message { get; set; }
        public required string Token { get; set; }
        public required UserDto User { get; set; }
    }
}
