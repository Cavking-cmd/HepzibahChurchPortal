using System.Security.Cryptography;
using ChurchPortal.Core.Dtos;
using ChurchPortal.Core.Dtos.UserDtos;
using ChurchPortal.Core.Entities;
using ChurchPortal.Repositories.Interfaces;
using ChurchPortal.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ChurchPortal.Services.Implementations
{
    public class UserService : IUserService
    {
        private const int VerificationTokenHours = 48;
        private const int ResetTokenHours = 2;
        private static readonly TimeSpan ResendCooldown = TimeSpan.FromSeconds(60);

        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;
        private readonly EmailOptions _emailOptions;
        private readonly ILogger<UserService> _logger;
        private readonly IMemoryCache _cache;

        private const int MaxAvatarBytes = 2 * 1024 * 1024;

        public UserService(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IUserRoleRepository userRoleRepository,
            IAuditLogRepository auditLogRepository,
            IUnitOfWork unitOfWork,
            IEmailSender emailSender,
            IOptions<EmailOptions> emailOptions,
            ILogger<UserService> logger,
            IMemoryCache cache)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _userRoleRepository = userRoleRepository;
            _auditLogRepository = auditLogRepository;
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
            _emailOptions = emailOptions.Value;
            _logger = logger;
            _cache = cache;
        }

        private static UserDto ToDto(User user) => new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            AvatarBase64 = user.AvatarFile != null && user.AvatarFile.Length > 0
                ? $"data:{user.AvatarMimeType};base64,{Convert.ToBase64String(user.AvatarFile)}"
                : null,
            EmailVerified = user.EmailVerified,
            UserRoles = user.UserRoles.Where(ur => !ur.IsDeleted && ur.Role != null && !ur.Role.IsDeleted).Select(ur => ur.Role!.Name).ToList()
        };

        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null) return false;

                await _userRepository.SoftDeleteAsync(user);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task<BaseResponse<UserDto>> LoginAsync(LoginRequestModel loginRequest)
        {
            try
            {
                if (Validator.CheckNull(loginRequest) || Validator.CheckString(loginRequest.Email) || Validator.CheckString(loginRequest.Password))
                {
                    return new BaseResponse<UserDto> { Message = "Email and password are required.", Status = false, Data = null };
                }

                var user = await _userRepository.GetUserByEmailAsync(loginRequest.Email);
                if (user == null)
                {
                    return new BaseResponse<UserDto> { Message = "Invalid email or password.", Status = false, Data = null };
                }

                if (!BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.Password))
                {
                    return new BaseResponse<UserDto> { Message = "Invalid email or password.", Status = false, Data = null };
                }

                return new BaseResponse<UserDto>
                {
                    Message = "Login successful.",
                    Status = true,
                    Data = new UserDto
                    {
                        Id = user.Id,
                        Email = user.Email,
                        UserRoles = user.UserRoles.Where(ur => !ur.IsDeleted && ur.Role != null && !ur.Role.IsDeleted).Select(ur => ur.Role!.Name).ToList()
                    }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new BaseResponse<UserDto> { Message = "An error occurred while logging in.", Status = false, Data = null };
            }
        }

        public async Task<BaseResponse<UserDto>> RegisterAsync(RegisterRequestModel registerRequest)
        {
            try
            {
                if (Validator.CheckNull(registerRequest) || Validator.CheckString(registerRequest.Email) || Validator.CheckString(registerRequest.Password))
                {
                    return new BaseResponse<UserDto> { Message = "Email and password are required.", Status = false, Data = null };
                }
                if (registerRequest.Roles == null || registerRequest.Roles.Count == 0)
                {
                    return new BaseResponse<UserDto> { Message = "At least one role must be explicitly specified.", Status = false, Data = null };
                }

                var exists = await _userRepository.CheckAsync(a => a.Email == registerRequest.Email && !a.IsDeleted);
                if (Validator.CheckDuplicate(exists))
                {
                    return new BaseResponse<UserDto> { Message = "A user with this email already exists.", Status = false, Data = null };
                }

                var roles = new List<Role>();
                foreach (var roleName in registerRequest.Roles.Distinct())
                {
                    var role = await _roleRepository.GetRoleByNameAsync(roleName);
                    if (role == null)
                    {
                        return new BaseResponse<UserDto> { Message = $"Role '{roleName}' does not exist.", Status = false, Data = null };
                    }
                    roles.Add(role);
                }

                var user = new User
                {
                    Email = registerRequest.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password),
                    EmailVerificationToken = GenerateToken(),
                    EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(VerificationTokenHours)
                };

                await _userRepository.CreateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                foreach (var role in roles)
                {
                    await _userRoleRepository.CreateAsync(new UserRole { UserId = user.Id, RoleId = role.Id });
                }
                await _unitOfWork.SaveChangesAsync();

                var emailError = _emailSender.IsConfigured
                    ? await SendVerificationEmailAsync(user)
                    : "Email delivery is not configured on the server.";

                return new BaseResponse<UserDto>
                {
                    Message = emailError == null
                        ? $"User registered successfully. A verification email has been sent to {user.Email}."
                        : $"User registered successfully, but the verification email to {user.Email} was not sent: {emailError} They can resend it from Settings once email is working.",
                    Status = true,
                    Data = new UserDto
                    {
                        Id = user.Id,
                        Email = user.Email,
                        EmailVerified = user.EmailVerified,
                        UserRoles = roles.Select(r => r.Name).ToList()
                    }
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<UserDto> { Message = ex.Message, Status = false, Data = null };
            }
        }

        public async Task<BaseResponse<List<UserDto>>> GetAllAsync()
        {
            try
            {
                var users = await _userRepository.GetAllUsersAsync();
                return new BaseResponse<List<UserDto>>
                {
                    Message = "OK",
                    Status = true,
                    Data = users.Select(ToDto).ToList()
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new BaseResponse<List<UserDto>>
                {
                    Message = "An error occurred while retrieving users.",
                    Status = false,
                    Data = null
                };
            }
        }

        public async Task<BaseResponse<UserDto>> GetMeAsync(Guid userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return new BaseResponse<UserDto> { Message = "User not found.", Status = false, Data = null };
                }

                return new BaseResponse<UserDto> { Message = "OK", Status = true, Data = ToDto(user) };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new BaseResponse<UserDto> { Message = "An error occurred while retrieving the profile.", Status = false, Data = null };
            }
        }

        public async Task<BaseResponse<UserDto>> UpdateProfileAsync(Guid userId, UpdateProfileRequest request)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return new BaseResponse<UserDto> { Message = "User not found.", Status = false, Data = null };
                }

                user.DisplayName = request.DisplayName;

                if (!string.IsNullOrWhiteSpace(request.AvatarBase64))
                {
                    var (mimeType, bytes, error) = ParseDataUri(request.AvatarBase64);
                    if (error != null)
                    {
                        return new BaseResponse<UserDto> { Message = error, Status = false, Data = null };
                    }
                    if (bytes!.Length > MaxAvatarBytes)
                    {
                        return new BaseResponse<UserDto> { Message = "Avatar image exceeds the 2MB size limit.", Status = false, Data = null };
                    }
                    user.AvatarFile = bytes;
                    user.AvatarMimeType = mimeType;
                }
                else if (request.AvatarBase64 == null)
                {
                }

                await _userRepository.Update(user);

                return new BaseResponse<UserDto> { Message = "Profile updated successfully.", Status = true, Data = ToDto(user) };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new BaseResponse<UserDto> { Message = "An error occurred while updating the profile.", Status = false, Data = null };
            }
        }

        private static (string? mimeType, byte[]? bytes, string? error) ParseDataUri(string dataUri)
        {
            try
            {
                var commaIndex = dataUri.IndexOf(',');
                if (!dataUri.StartsWith("data:") || commaIndex < 0)
                {
                    return (null, null, "Avatar must be a valid base64 data URI.");
                }

                var header = dataUri.Substring(5, commaIndex - 5);
                var mimeType = header.Split(';')[0];
                var base64Data = dataUri.Substring(commaIndex + 1);
                var bytes = Convert.FromBase64String(base64Data);
                return (string.IsNullOrWhiteSpace(mimeType) ? "image/png" : mimeType, bytes, null);
            }
            catch (FormatException)
            {
                return (null, null, "Avatar base64 payload is not valid.");
            }
        }

        public async Task<BaseResponse<UserDto>> UpdateEmailAsync(Guid userId, UpdateEmailRequest request)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return new BaseResponse<UserDto> { Message = "User not found.", Status = false, Data = null };
                }

                if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.Password))
                {
                    return new BaseResponse<UserDto> { Message = "Current password is incorrect.", Status = false, Data = null };
                }

                var taken = await _userRepository.CheckAsync(u => u.Email == request.NewEmail && u.Id != userId && !u.IsDeleted);
                if (taken)
                {
                    return new BaseResponse<UserDto> { Message = "That email is already in use by another account.", Status = false, Data = null };
                }

                user.Email = request.NewEmail;
                await _userRepository.Update(user);

                return new BaseResponse<UserDto> { Message = "Email updated successfully. Please log in again.", Status = true, Data = ToDto(user) };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new BaseResponse<UserDto> { Message = "An error occurred while updating the email.", Status = false, Data = null };
            }
        }

        public async Task<BaseResponse<bool>> UpdatePasswordAsync(Guid userId, UpdatePasswordRequest request)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return new BaseResponse<bool> { Message = "User not found.", Status = false, Data = false };
                }

                if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.Password))
                {
                    return new BaseResponse<bool> { Message = "Current password is incorrect.", Status = false, Data = false };
                }

                user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                await _userRepository.Update(user);

                await _auditLogRepository.CreateAsync(new AuditLog
                {
                    EntityName = nameof(User),
                    EntityId = user.Id,
                    Action = "PasswordChanged",
                    UserId = userId
                });
                await _unitOfWork.SaveChangesAsync();

                return new BaseResponse<bool> { Message = "Password changed successfully.", Status = true, Data = true };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new BaseResponse<bool> { Message = "An error occurred while updating the password.", Status = false, Data = false };
            }
        }

        public async Task<BaseResponse<bool>> ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            try
            {
                if (Validator.CheckNull(request) || Validator.CheckString(request.Email))
                {
                    return new BaseResponse<bool> { Message = "Email is required.", Status = false, Data = false };
                }

                if (!_emailSender.IsConfigured)
                {
                    return new BaseResponse<bool>
                    {
                        Message = "Email delivery is not configured on the server. Ask an administrator to add a Resend API key.",
                        Status = false,
                        Data = false
                    };
                }

                var user = await _userRepository.GetUserByEmailAsync(request.Email.Trim());

                // Always report success so the endpoint cannot be used to enumerate accounts.
                if (user == null)
                {
                    return new BaseResponse<bool>
                    {
                        Message = "If an account with that email exists, a reset link has been sent.",
                        Status = true,
                        Data = true
                    };
                }

                user.PasswordResetToken = GenerateToken();
                user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(ResetTokenHours);
                await _userRepository.Update(user);

                var link = BuildFrontendLink($"/reset-password?token={user.PasswordResetToken}");
                await SafeSendAsync(
                    user.Email,
                    "Reset your Hephzibah Church portal password",
                    EmailTemplates.ResetPassword(user.DisplayName ?? user.Email, link, ResetTokenHours));

                await _auditLogRepository.CreateAsync(new AuditLog
                {
                    EntityName = nameof(User),
                    EntityId = user.Id,
                    Action = "PasswordResetRequested",
                    UserId = user.Id
                });
                await _unitOfWork.SaveChangesAsync();

                return new BaseResponse<bool>
                {
                    Message = "If an account with that email exists, a reset link has been sent.",
                    Status = true,
                    Data = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Forgot password failed for {Email}.", request.Email);
                return new BaseResponse<bool> { Message = "An error occurred while requesting a password reset.", Status = false, Data = false };
            }
        }

        public async Task<BaseResponse<bool>> ResetPasswordAsync(ResetPasswordRequest request)
        {
            try
            {
                if (Validator.CheckNull(request) || Validator.CheckString(request.Token) || Validator.CheckString(request.NewPassword))
                {
                    return new BaseResponse<bool> { Message = "A reset token and a new password are required.", Status = false, Data = false };
                }

                if (request.NewPassword.Length < 6)
                {
                    return new BaseResponse<bool> { Message = "Your new password must be at least 6 characters.", Status = false, Data = false };
                }

                var user = await _userRepository.GetByPasswordResetTokenAsync(request.Token.Trim());
                if (user == null || user.PasswordResetTokenExpiry == null || user.PasswordResetTokenExpiry < DateTime.UtcNow)
                {
                    return new BaseResponse<bool> { Message = "This reset link is invalid or has expired.", Status = false, Data = false };
                }

                user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                user.PasswordResetToken = null;
                user.PasswordResetTokenExpiry = null;
                await _userRepository.Update(user);

                await _auditLogRepository.CreateAsync(new AuditLog
                {
                    EntityName = nameof(User),
                    EntityId = user.Id,
                    Action = "PasswordReset",
                    UserId = user.Id
                });
                await _unitOfWork.SaveChangesAsync();

                return new BaseResponse<bool> { Message = "Your password has been reset. You can now sign in.", Status = true, Data = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Reset password failed.");
                return new BaseResponse<bool> { Message = "An error occurred while resetting the password.", Status = false, Data = false };
            }
        }

        public async Task<BaseResponse<bool>> VerifyEmailAsync(VerifyEmailRequest request)
        {
            try
            {
                if (Validator.CheckNull(request) || Validator.CheckString(request.Token))
                {
                    return new BaseResponse<bool> { Message = "A verification token is required.", Status = false, Data = false };
                }

                var user = await _userRepository.GetByEmailVerificationTokenAsync(request.Token.Trim());
                if (user == null)
                {
                    return new BaseResponse<bool> { Message = "This verification link is invalid.", Status = false, Data = false };
                }

                if (!user.EmailVerified &&
                    (user.EmailVerificationTokenExpiry == null || user.EmailVerificationTokenExpiry < DateTime.UtcNow))
                {
                    return new BaseResponse<bool> { Message = "This verification link has expired. Request a new one.", Status = false, Data = false };
                }

                user.EmailVerified = true;
                user.EmailVerificationToken = null;
                user.EmailVerificationTokenExpiry = null;
                await _userRepository.Update(user);

                await _auditLogRepository.CreateAsync(new AuditLog
                {
                    EntityName = nameof(User),
                    EntityId = user.Id,
                    Action = "EmailVerified",
                    UserId = user.Id
                });
                await _unitOfWork.SaveChangesAsync();

                return new BaseResponse<bool> { Message = "Thank you. Your email address is verified.", Status = true, Data = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Email verification failed.");
                return new BaseResponse<bool> { Message = "An error occurred while verifying the email.", Status = false, Data = false };
            }
        }

        public async Task<BaseResponse<VerificationEmailSentDto>> ResendVerificationAsync(Guid userId, ResendVerificationRequest? request)
        {
            try
            {
                if (!_emailSender.IsConfigured)
                {
                    return new BaseResponse<VerificationEmailSentDto>
                    {
                        Message = "Email delivery is not configured on the server. Ask an administrator to add a Resend API key.",
                        Status = false
                    };
                }

                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null || user.IsDeleted)
                {
                    return new BaseResponse<VerificationEmailSentDto>
                    {
                        Message = "We could not find your account.",
                        Status = false
                    };
                }

                // Extra check: the person must type the address the link is going to.
                // It has to match the signed-in account, so a borrowed session or a
                // mistyped address cannot trigger mail to the wrong inbox unnoticed.
                var confirmEmail = request?.ConfirmEmail?.Trim();
                if (string.IsNullOrWhiteSpace(confirmEmail))
                {
                    return new BaseResponse<VerificationEmailSentDto>
                    {
                        Message = "Please type your email address to confirm where the link should go.",
                        Status = false
                    };
                }

                if (!string.Equals(confirmEmail, user.Email.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Resend verification refused for user {UserId}: confirmation address did not match.", userId);
                    return new BaseResponse<VerificationEmailSentDto>
                    {
                        Message = "That address does not match the email on your account. Check it and try again.",
                        Status = false
                    };
                }

                if (user.EmailVerified)
                {
                    return new BaseResponse<VerificationEmailSentDto>
                    {
                        Message = "This email address is already verified.",
                        Status = true,
                        Data = new VerificationEmailSentDto { Email = user.Email, SentAtUtc = DateTime.UtcNow }
                    };
                }

                var cooldownKey = $"resend-verification:{user.Id}";
                if (_cache.TryGetValue(cooldownKey, out DateTime lastSentUtc))
                {
                    var wait = (int)Math.Ceiling((lastSentUtc + ResendCooldown - DateTime.UtcNow).TotalSeconds);
                    if (wait > 0)
                    {
                        return new BaseResponse<VerificationEmailSentDto>
                        {
                            Message = $"A verification email was just sent to {user.Email}. Please wait {wait} seconds before asking for another.",
                            Status = false
                        };
                    }
                }

                user.EmailVerificationToken = GenerateToken();
                user.EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(VerificationTokenHours);
                await _userRepository.Update(user);

                var error = await SendVerificationEmailAsync(user);
                if (error != null)
                {
                    return new BaseResponse<VerificationEmailSentDto>
                    {
                        Message = $"The verification email to {user.Email} could not be sent. {error}",
                        Status = false
                    };
                }

                var sentAt = DateTime.UtcNow;
                _cache.Set(cooldownKey, sentAt, ResendCooldown);

                await _auditLogRepository.CreateAsync(new AuditLog
                {
                    EntityName = nameof(User),
                    EntityId = user.Id,
                    Action = "VerificationEmailResent",
                    UserId = user.Id
                });
                await _unitOfWork.SaveChangesAsync();

                return new BaseResponse<VerificationEmailSentDto>
                {
                    Message = $"Verification email sent to {user.Email}.",
                    Status = true,
                    Data = new VerificationEmailSentDto { Email = user.Email, SentAtUtc = sentAt }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Resend verification failed for user {UserId}.", userId);
                return new BaseResponse<VerificationEmailSentDto>
                {
                    Message = "An error occurred while resending the verification email.",
                    Status = false
                };
            }
        }

        public BaseResponse<EmailStatusDto> GetEmailStatus()
        {
            return new BaseResponse<EmailStatusDto>
            {
                Message = _emailSender.IsConfigured
                    ? "Email delivery is configured."
                    : "Email delivery is not configured.",
                Status = true,
                Data = new EmailStatusDto
                {
                    Configured = _emailSender.IsConfigured,
                    Provider = "Resend",
                    From = _emailOptions.From,
                    FromName = _emailOptions.ResendFromName,
                    FrontendBaseUrl = _emailOptions.FrontendBaseUrl
                }
            };
        }

        public async Task<BaseResponse<VerificationEmailSentDto>> SendTestEmailAsync(Guid userId)
        {
            try
            {
                if (!_emailSender.IsConfigured)
                {
                    return new BaseResponse<VerificationEmailSentDto>
                    {
                        Message = "Email delivery is not configured on the server. Ask an administrator to add a Resend API key.",
                        Status = false
                    };
                }

                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null || user.IsDeleted)
                {
                    return new BaseResponse<VerificationEmailSentDto>
                    {
                        Message = "We could not find your account.",
                        Status = false
                    };
                }

                var error = await SafeSendAsync(
                    user.Email,
                    "Hephzibah Church portal test email",
                    EmailTemplates.TestEmail(user.DisplayName ?? user.Email));

                if (error != null)
                {
                    return new BaseResponse<VerificationEmailSentDto>
                    {
                        Message = $"The test email to {user.Email} could not be sent. {error}",
                        Status = false
                    };
                }

                return new BaseResponse<VerificationEmailSentDto>
                {
                    Message = $"Test email sent to {user.Email}.",
                    Status = true,
                    Data = new VerificationEmailSentDto { Email = user.Email, SentAtUtc = DateTime.UtcNow }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Test email failed for user {UserId}.", userId);
                return new BaseResponse<VerificationEmailSentDto>
                {
                    Message = "An error occurred while sending the test email.",
                    Status = false
                };
            }
        }

        private static string GenerateToken()
        {
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        }

        private string BuildFrontendLink(string path)
        {
            var baseUrl = string.IsNullOrWhiteSpace(_emailOptions.FrontendBaseUrl)
                ? "http://localhost:5173"
                : _emailOptions.FrontendBaseUrl.TrimEnd('/');
            return $"{baseUrl}{path}";
        }

        /// <returns>null when the message was accepted, otherwise the reason it failed.</returns>
        private async Task<string?> SendVerificationEmailAsync(User user)
        {
            if (string.IsNullOrWhiteSpace(user.EmailVerificationToken))
            {
                return "No verification token was generated.";
            }

            var link = BuildFrontendLink($"/verify-email?token={user.EmailVerificationToken}");
            return await SafeSendAsync(
                user.Email,
                "Verify your Hephzibah Church portal email",
                EmailTemplates.VerifyEmail(user.DisplayName ?? user.Email, link, VerificationTokenHours));
        }

        /// <returns>null when the message was accepted, otherwise the reason it failed.</returns>
        private async Task<string?> SafeSendAsync(string to, string subject, string body)
        {
            try
            {
                await _emailSender.SendAsync(to, subject, body);
                return null;
            }
            catch (EmailDeliveryException ex)
            {
                _logger.LogError(ex, "Failed to send '{Subject}' email to {To}.", subject, to);
                return ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send '{Subject}' email to {To}.", subject, to);
                return "Email delivery is not available right now. Please try again later.";
            }
        }
    }
}
