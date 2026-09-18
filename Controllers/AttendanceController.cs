using System.Security.Claims;
using ChurchPortal.Core.Dtos.Request;
using ChurchPortal.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChurchPortal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;

        public AttendanceController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        private Guid CurrentUserId =>
            Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : Guid.Empty;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _attendanceService.GetAll();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var result = await _attendanceService.GetAsync(id);
            if (result.Status) return Ok(result);
            return NotFound(result);
        }

        [Authorize(Roles = "Admin,AttendanceOfficer")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAttendanceRequestModel model)
        {
            var result = await _attendanceService.CreateAsync(model, CurrentUserId);
            if (result.Status) return Ok(result);
            return BadRequest(result);
        }

        [Authorize(Roles = "Admin,AttendanceOfficer")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAttendanceModel model)
        {
            if (id != model.Id) return BadRequest("Route id and body id must match.");
            var result = await _attendanceService.UpdateAsync(model, CurrentUserId);
            if (result.Status) return Ok(result);
            return BadRequest(result);
        }

        [Authorize(Roles = "Admin,AttendanceOfficer")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _attendanceService.SoftDeleteAsync(id, CurrentUserId);
            if (result.Status) return NoContent();
            return NotFound(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{id}/approve")]
        public async Task<IActionResult> Approve(Guid id)
        {
            var result = await _attendanceService.ApproveAsync(id, CurrentUserId);
            if (result.Status) return Ok(result);
            return BadRequest(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{id}/lock")]
        public async Task<IActionResult> Lock(Guid id)
        {
            var result = await _attendanceService.LockAsync(id, CurrentUserId);
            if (result.Status) return Ok(result);
            return BadRequest(result);
        }
    }
}
