using System.Security.Claims;
using ChurchPortal.Services.Interfaces;
using Fido2NetLib;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChurchPortal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PasskeyController : ControllerBase
    {
        private readonly IPasskeyService _passkeyService;

        public PasskeyController(IPasskeyService passkeyService)
        {
            _passkeyService = passkeyService;
        }

        private Guid CurrentUserId =>
            Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : Guid.Empty;

        [Authorize]
        [HttpPost("register/options")]
        public async Task<IActionResult> RegisterOptions()
        {
            try
            {
                var options = await _passkeyService.GetRegisterOptionsAsync(CurrentUserId);
                return Ok(options);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("register/complete")]
        public async Task<IActionResult> RegisterComplete([FromBody] PasskeyRegisterCompleteRequest model)
        {
            var result = await _passkeyService.CompleteRegisterAsync(CurrentUserId, model.AttestationResponse, model.Nickname);
            if (result.Status) return Ok(result);
            return BadRequest(result);
        }

        [AllowAnonymous]
        [HttpPost("login/options")]
        public async Task<IActionResult> LoginOptions([FromBody] PasskeyLoginOptionsRequest model)
        {
            try
            {
                var options = await _passkeyService.GetLoginOptionsAsync(model.Email);
                return Ok(options);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("login/complete")]
        public async Task<IActionResult> LoginComplete([FromBody] PasskeyLoginCompleteRequest model)
        {
            var result = await _passkeyService.CompleteLoginAsync(model.Email, model.AssertionResponse);
            if (result.Status && result.Data != null) return Ok(result.Data);
            return Unauthorized(result);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> List()
        {
            var result = await _passkeyService.ListAsync(CurrentUserId);
            return Ok(result);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _passkeyService.DeleteAsync(CurrentUserId, id);
            if (result) return NoContent();
            return NotFound(new { message = "Passkey not found." });
        }
    }
}
