using ChurchPortal.AuthService;
using ChurchPortal.Core.Dtos;
using ChurchPortal.Core.Dtos.UserDtos;
using ChurchPortal.Core.Entities;
using ChurchPortal.Repositories.Interfaces;
using ChurchPortal.Services.Interfaces;
using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.Extensions.Caching.Memory;
using System.Linq;

namespace ChurchPortal.Services.Implementations
{
    public class PasskeyService : IPasskeyService
    {
        private readonly IFido2 _fido2;
        private readonly IUserRepository _userRepository;
        private readonly IUserCredentialRepository _credentialRepository;
        private readonly IAuthService _authService;
        private readonly IMemoryCache _cache;
        private readonly IUnitOfWork _unitOfWork;

        public PasskeyService(
            IFido2 fido2,
            IUserRepository userRepository,
            IUserCredentialRepository credentialRepository,
            IAuthService authService,
            IMemoryCache cache,
            IUnitOfWork unitOfWork)
        {
            _fido2 = fido2;
            _userRepository = userRepository;
            _credentialRepository = credentialRepository;
            _authService = authService;
            _cache = cache;
            _unitOfWork = unitOfWork;
        }

        private static string RegisterCacheKey(Guid userId) => $"fido2-register-{userId}";
        private static string LoginCacheKey(string email) => $"fido2-login-{email.ToLowerInvariant()}";

        public async Task<CredentialCreateOptions> GetRegisterOptionsAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId)
                ?? throw new InvalidOperationException("User not found.");

            var existingCredentials = (await _credentialRepository.GetAllAsync(c => c.UserId == userId && !c.IsDeleted))
                .Select(c => new PublicKeyCredentialDescriptor(c.CredentialId))
                .ToList();

            var fido2User = new Fido2User
            {
                Id = user.Id.ToByteArray(),
                Name = user.Email,
                DisplayName = string.IsNullOrWhiteSpace(user.DisplayName) ? user.Email : user.DisplayName
            };

            var options = _fido2.RequestNewCredential(new RequestNewCredentialParams
            {
                User = fido2User,
                ExcludeCredentials = existingCredentials,
                AuthenticatorSelection = new AuthenticatorSelection
                {
                    ResidentKey = ResidentKeyRequirement.Preferred,
                    UserVerification = UserVerificationRequirement.Preferred
                },
                AttestationPreference = AttestationConveyancePreference.None
            });

            _cache.Set(RegisterCacheKey(userId), options, TimeSpan.FromMinutes(5));

            return options;
        }

        public async Task<BaseResponse<bool>> CompleteRegisterAsync(Guid userId, AuthenticatorAttestationRawResponse attestationResponse, string? nickname)
        {
            try
            {
                if (!_cache.TryGetValue(RegisterCacheKey(userId), out CredentialCreateOptions? options) || options == null)
                {
                    return new BaseResponse<bool> { Message = "Registration session expired. Please try again.", Status = false, Data = false };
                }

                var result = await _fido2.MakeNewCredentialAsync(new MakeNewCredentialParams
                {
                    AttestationResponse = attestationResponse,
                    OriginalOptions = options,
                    IsCredentialIdUniqueToUserCallback = async (args, _) =>
                        !(await _credentialRepository.CheckAsync(c => c.CredentialId == args.CredentialId))
                });

                var credential = new UserCredential
                {
                    UserId = userId,
                    CredentialId = result.Id,
                    PublicKey = result.PublicKey,
                    SignatureCounter = result.SignCount,
                    Nickname = string.IsNullOrWhiteSpace(nickname) ? "Passkey" : nickname
                };

                await _credentialRepository.CreateAsync(credential);
                await _unitOfWork.SaveChangesAsync();

                _cache.Remove(RegisterCacheKey(userId));

                return new BaseResponse<bool> { Message = "Passkey registered successfully.", Status = true, Data = true };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new BaseResponse<bool> { Message = $"Failed to register passkey: {ex.Message}", Status = false, Data = false };
            }
        }

        public async Task<AssertionOptions> GetLoginOptionsAsync(string email)
        {
            var user = await _userRepository.GetUserByEmailAsync(email)
                ?? throw new InvalidOperationException("No account found for that email.");

            var credentials = (await _credentialRepository.GetAllAsync(c => c.UserId == user.Id && !c.IsDeleted))
                .Select(c => new PublicKeyCredentialDescriptor(c.CredentialId))
                .ToList();

            var options = _fido2.GetAssertionOptions(new GetAssertionOptionsParams
            {
                AllowedCredentials = credentials,
                UserVerification = UserVerificationRequirement.Preferred
            });

            _cache.Set(LoginCacheKey(email), options, TimeSpan.FromMinutes(5));

            return options;
        }

        public async Task<BaseResponse<object>> CompleteLoginAsync(string email, AuthenticatorAssertionRawResponse assertionResponse)
        {
            try
            {
                if (!_cache.TryGetValue(LoginCacheKey(email), out AssertionOptions? options) || options == null)
                {
                    return new BaseResponse<object> { Message = "Login session expired. Please try again.", Status = false, Data = null };
                }

                var user = await _userRepository.GetUserByEmailAsync(email);
                if (user == null)
                {
                    return new BaseResponse<object> { Message = "Invalid login.", Status = false, Data = null };
                }

                var credentialIdBytes = assertionResponse.RawId;
                var storedCredential = (await _credentialRepository.GetAllAsync(c => c.UserId == user.Id && !c.IsDeleted))
                    .FirstOrDefault(c => c.CredentialId.SequenceEqual(credentialIdBytes));

                if (storedCredential == null)
                {
                    return new BaseResponse<object> { Message = "Passkey not recognized.", Status = false, Data = null };
                }

                var result = await _fido2.MakeAssertionAsync(new MakeAssertionParams
                {
                    AssertionResponse = assertionResponse,
                    OriginalOptions = options,
                    StoredPublicKey = storedCredential.PublicKey,
                    StoredSignatureCounter = (uint)storedCredential.SignatureCounter,
                    IsUserHandleOwnerOfCredentialIdCallback = (args, _) =>
                        Task.FromResult(args.UserHandle.SequenceEqual(user.Id.ToByteArray()))
                });

                storedCredential.SignatureCounter = result.SignCount;
                await _credentialRepository.Update(storedCredential);
                await _unitOfWork.SaveChangesAsync();

                _cache.Remove(LoginCacheKey(email));

                var userDto = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    DisplayName = user.DisplayName,
                    AvatarBase64 = user.AvatarFile != null && user.AvatarFile.Length > 0
                        ? $"data:{user.AvatarMimeType};base64,{Convert.ToBase64String(user.AvatarFile)}"
                        : null,
                    UserRoles = user.UserRoles.Select(ur => ur.Role!.Name).ToList()
                };
                var token = _authService.GenerateToken(userDto);

                return new BaseResponse<object>
                {
                    Message = "Login successful.",
                    Status = true,
                    Data = new { message = "Login successful.", token, user = userDto }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new BaseResponse<object> { Message = $"Passkey login failed: {ex.Message}", Status = false, Data = null };
            }
        }

        public async Task<List<PasskeyDto>> ListAsync(Guid userId)
        {
            var credentials = await _credentialRepository.GetAllAsync(c => c.UserId == userId && !c.IsDeleted);
            return credentials.Select(c => new PasskeyDto
            {
                Id = c.Id,
                Nickname = c.Nickname,
                CreatedDate = c.CreatedDate
            }).ToList();
        }

        public async Task<bool> DeleteAsync(Guid userId, Guid credentialRowId)
        {
            var credential = await _credentialRepository.GetByIdAsync(credentialRowId);
            if (credential == null || credential.UserId != userId)
            {
                return false;
            }

            await _credentialRepository.SoftDeleteAsync(credential);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
