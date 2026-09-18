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
    public class FellowshipAttendanceController : ControllerBase
    {
        private readonly IFellowshipAttendanceService _fellowshipAttendanceService;

        public FellowshipAttendanceController(IFellowshipAttendanceService fellowshipAttendanceService)
        {
            _fellowshipAttendanceService = fellowshipAttendanceService;
        }

        private Guid CurrentUserId =>
            Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : Guid.Empty;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _fellowshipAttendanceService.GetAll();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var result = await _fellowshipAttendanceService.GetAsync(id);
            if (result.Status) return Ok(result);
            return NotFound(result);
        }

        [Authorize(Roles = "Admin,FellowshipLeader")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateFellowshipAttendanceRequestModel model)
        {
            var result = await _fellowshipAttendanceService.CreateAsync(model, CurrentUserId);
            if (result.Status) return Ok(result);
            return BadRequest(result);
        }

        [Authorize(Roles = "Admin,FellowshipLeader")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFellowshipAttendanceModel model)
        {
            if (id != model.Id) return BadRequest("Route id and body id must match.");
            var result = await _fellowshipAttendanceService.UpdateAsync(model, CurrentUserId);
            if (result.Status) return Ok(result);
            return BadRequest(result);
        }

        [Authorize(Roles = "Admin,FellowshipLeader")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _fellowshipAttendanceService.SoftDeleteAsync(id, CurrentUserId);
            if (result.Status) return NoContent();
            return NotFound(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{id}/approve")]
        public async Task<IActionResult> Approve(Guid id)
        {
            var result = await _fellowshipAttendanceService.ApproveAsync(id, CurrentUserId);
            if (result.Status) return Ok(result);
            return BadRequest(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{id}/lock")]
        public async Task<IActionResult> Lock(Guid id)
        {
            var result = await _fellowshipAttendanceService.LockAsync(id, CurrentUserId);
            if (result.Status) return Ok(result);
            return BadRequest(result);
        }
    }
}
