using System.Security.Claims;
using ChurchPortal.Core.Dtos;
using ChurchPortal.Core.Dtos.Request;
using ChurchPortal.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChurchPortal.Controllers
{
    /// <summary>Pastor name records. Managed by Admin, Accounts Admin and Attendance Officers.</summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,AccountsAdmin,AttendanceOfficer")]
    public class PastorsController : ControllerBase
    {
        private readonly IPastorService _pastorService;

        public PastorsController(IPastorService pastorService)
        {
            _pastorService = pastorService;
        }

        private Guid CurrentUserId =>
            Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : Guid.Empty;

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false, [FromQuery] string? search = null)
        {
            var result = await _pastorService.GetAllAsync(includeInactive, search);
            return result.Status ? Ok(result) : BadRequest(result);
        }

        [HttpGet("possible-duplicates")]
        public async Task<IActionResult> PossibleDuplicates([FromQuery] string? name, [FromQuery] Guid? excludeId = null)
        {
            var result = await _pastorService.FindPossibleDuplicatesAsync(name, excludeId);
            return result.Status ? Ok(result) : BadRequest(result);
        }

        /// <summary>409 with the matching records if the name looks like an existing pastor and ConfirmNotDuplicate is false.</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePastorModel model)
        {
            return ToResult(await _pastorService.CreateAsync(model, CurrentUserId));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePastorModel model)
        {
            if (id != model.Id) return BadRequest(new { message = "Route id and body id must match.", status = false });
            return ToResult(await _pastorService.UpdateAsync(model, CurrentUserId));
        }

        [HttpGet("legacy-preachers")]
        public async Task<IActionResult> LegacyPreachers()
        {
            var result = await _pastorService.GetLegacyPreachersAsync();
            return result.Status ? Ok(result) : BadRequest(result);
        }

        [HttpPost("link-legacy")]
        public async Task<IActionResult> LinkLegacy([FromBody] LinkLegacyPreacherModel model)
        {
            var result = await _pastorService.LinkLegacyPreacherAsync(model, CurrentUserId);
            return result.Status ? Ok(result) : BadRequest(result);
        }

        private IActionResult ToResult(PastorSaveResult result)
        {
            if (result.Duplicate != null)
            {
                return Conflict(new BaseResponse<PastorDuplicateDto>
                {
                    Message = result.Duplicate.Message,
                    Status = false,
                    Data = result.Duplicate
                });
            }

            return result.Response.Status ? Ok(result.Response) : BadRequest(result.Response);
        }
    }
}
