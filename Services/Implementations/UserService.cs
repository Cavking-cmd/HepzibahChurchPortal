using ChurchPortal.Core.Dtos;
using ChurchPortal.Core.Dtos.UserDtos;
using ChurchPortal.Core.Entities;
using ChurchPortal.Repositories.Interfaces;
using ChurchPortal.Services.Interfaces;

namespace ChurchPortal.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IUnitOfWork _unitOfWork;

        private const int MaxAvatarBytes = 2 * 1024 * 1024;

        public UserService(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IUserRoleRepository userRoleRepository,
            IAuditLogRepository auditLogRepository,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _userRoleRepository = userRoleRepository;
            _auditLogRepository = auditLogRepository;
            _unitOfWork = unitOfWork;
        }

        private static UserDto ToDto(User user) => new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            AvatarBase64 = user.AvatarFile != null && user.AvatarFile.Length > 0
                ? $"data:{user.AvatarMimeType};base64,{Convert.ToBase64String(user.AvatarFile)}"
                : null,
            UserRoles = user.UserRoles.Select(ur => ur.Role!.Name).ToList()
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
                        UserRoles = user.UserRoles.Select(ur => ur.Role!.Name).ToList()
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
                    Password = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password)
                };

                await _userRepository.CreateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                foreach (var role in roles)
                {
                    await _userRoleRepository.CreateAsync(new UserRole { UserId = user.Id, RoleId = role.Id });
                }
                await _unitOfWork.SaveChangesAsync();

                return new BaseResponse<UserDto>
                {
                    Message = "User registered successfully.",
                    Status = true,
                    Data = new UserDto
                    {
                        Id = user.Id,
                        Email = user.Email,
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
    }
}
