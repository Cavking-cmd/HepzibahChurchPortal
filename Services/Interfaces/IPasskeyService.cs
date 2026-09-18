using ChurchPortal.Core.Dtos;
using Fido2NetLib;

namespace ChurchPortal.Services.Interfaces
{
    public interface IPasskeyService
    {
        Task<CredentialCreateOptions> GetRegisterOptionsAsync(Guid userId);
        Task<BaseResponse<bool>> CompleteRegisterAsync(Guid userId, AuthenticatorAttestationRawResponse attestationResponse, string? nickname);
        Task<AssertionOptions> GetLoginOptionsAsync(string email);
        Task<BaseResponse<object>> CompleteLoginAsync(string email, AuthenticatorAssertionRawResponse assertionResponse);
        Task<List<PasskeyDto>> ListAsync(Guid userId);
        Task<bool> DeleteAsync(Guid userId, Guid credentialRowId);
    }

    public class PasskeyDto
    {
        public Guid Id { get; set; }
        public string? Nickname { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class PasskeyRegisterCompleteRequest
    {
        public required AuthenticatorAttestationRawResponse AttestationResponse { get; set; }
        public string? Nickname { get; set; }
    }

    public class PasskeyLoginOptionsRequest
    {
        public required string Email { get; set; }
    }

    public class PasskeyLoginCompleteRequest
    {
        public required string Email { get; set; }
        public required AuthenticatorAssertionRawResponse AssertionResponse { get; set; }
    }
}
