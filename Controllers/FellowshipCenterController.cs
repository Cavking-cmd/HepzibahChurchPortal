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
    public class FellowshipCenterController : ControllerBase
    {
        private readonly IFellowshipCenterService _fellowshipCenterService;

        public FellowshipCenterController(IFellowshipCenterService fellowshipCenterService)
        {
            _fellowshipCenterService = fellowshipCenterService;
        }

        private Guid CurrentUserId =>
            Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : Guid.Empty;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _fellowshipCenterService.GetAll();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var result = await _fellowshipCenterService.GetAsync(id);
            if (result.Status) return Ok(result);
            return NotFound(result);
        }

        [Authorize(Roles = "Admin,FellowshipLeader")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateFellowshipCenterRequestModel model)
        {
            var result = await _fellowshipCenterService.CreateAsync(model, CurrentUserId);
            if (result.Status) return Ok(result);
            return BadRequest(result);
        }

        [Authorize(Roles = "Admin,FellowshipLeader")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFellowshipCenterModel model)
        {
            if (id != model.Id) return BadRequest("Route id and body id must match.");
            var result = await _fellowshipCenterService.UpdateAsync(model, CurrentUserId);
            if (result.Status) return Ok(result);
            return BadRequest(result);
        }

        [Authorize(Roles = "Admin,FellowshipLeader")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _fellowshipCenterService.SoftDeleteAsync(id, CurrentUserId);
            if (result.Status) return NoContent();
            return NotFound(result);
        }
    }
}
