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
    public class ServiceController : ControllerBase
    {
        private readonly IServiceService _serviceService;

        public ServiceController(IServiceService serviceService)
        {
            _serviceService = serviceService;
        }

        private Guid CurrentUserId =>
            Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : Guid.Empty;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _serviceService.GetAll();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var result = await _serviceService.GetAsync(id);
            if (result.Status) return Ok(result);
            return NotFound(result);
        }

        [Authorize(Roles = "Admin,AttendanceOfficer")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateServiceRequestModel model)
        {
            var result = await _serviceService.CreateAsync(model, CurrentUserId);
            if (result.Status) return Ok(result);
            return BadRequest(result);
        }

        [Authorize(Roles = "Admin,AttendanceOfficer")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateServiceModel model)
        {
            if (id != model.Id) return BadRequest("Route id and body id must match.");
            var result = await _serviceService.UpdateAsync(model, CurrentUserId);
            if (result.Status) return Ok(result);
            return BadRequest(result);
        }

        [Authorize(Roles = "Admin,AttendanceOfficer")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _serviceService.SoftDeleteAsync(id, CurrentUserId);
            if (result.Status) return NoContent();
            return NotFound(result);
        }
    }
}
