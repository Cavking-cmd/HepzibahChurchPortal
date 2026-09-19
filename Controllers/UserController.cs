using System.Security.Claims;
using ChurchPortal.AuthService;
using ChurchPortal.Core.Dtos.UserDtos;
using ChurchPortal.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChurchPortal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private Guid CurrentUserId =>
            Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : Guid.Empty;

        private readonly IUserService _userService;
        private readonly IAuthService _authService;

        public UserController(IUserService userService, IAuthService authService)
        {
            _userService = userService;
            _authService = authService;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestModel model)
        {
            var result = await _userService.RegisterAsync(model);
            if (result.Status)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestModel model)
        {
            var result = await _userService.LoginAsync(model);
            if (result.Status && result.Data != null)
            {
                string token = _authService.GenerateToken(result.Data);
                return Ok(new
                {
                    message = result.Message,
                    token = token,
                    user = result.Data
                });
            }

            return Unauthorized(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _userService.GetAllAsync();
            if (result.Status)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _userService.DeleteAsync(id);
            if (result)
            {
                return NoContent();
            }

            return NotFound(new { Message = "User not found." });
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var result = await _userService.GetMeAsync(CurrentUserId);
            if (result.Status) return Ok(result.Data);
            return NotFound(result);
        }

        [Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest model)
        {
            var result = await _userService.UpdateProfileAsync(CurrentUserId, model);
            if (result.Status) return Ok(result);
            return BadRequest(result);
        }

        [Authorize]
        [HttpPut("email")]
        public async Task<IActionResult> UpdateEmail([FromBody] UpdateEmailRequest model)
        {
            var result = await _userService.UpdateEmailAsync(CurrentUserId, model);
            if (result.Status) return Ok(result);
            return BadRequest(result);
        }

        [Authorize]
        [HttpPut("password")]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordRequest model)
        {
            var result = await _userService.UpdatePasswordAsync(CurrentUserId, model);
            if (result.Status) return Ok(result);
            return BadRequest(result);
        }
    }
}
