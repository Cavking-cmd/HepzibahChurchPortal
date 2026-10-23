using System.Security.Claims;
using ChurchPortal.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChurchPortal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly IReimbursementService _reimbursementService;

        public NotificationController(IReimbursementService reimbursementService)
        {
            _reimbursementService = reimbursementService;
        }

        private Guid CurrentUserId =>
            Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : Guid.Empty;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _reimbursementService.GetNotificationsAsync(CurrentUserId);
            return Ok(result);
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> UnreadCount()
        {
            var result = await _reimbursementService.GetUnreadNotificationCountAsync(CurrentUserId);
            return Ok(result);
        }

        [HttpPost("read-all")]
        public async Task<IActionResult> MarkAllRead()
        {
            var result = await _reimbursementService.MarkNotificationsReadAsync(CurrentUserId);
            return Ok(result);
        }
    }
}
