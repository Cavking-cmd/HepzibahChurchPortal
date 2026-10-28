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
    public class DepartmentsController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentsController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        private Guid CurrentUserId =>
            Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : Guid.Empty;

        /// <summary>Active departments for the reimbursement form. Names only, no budget figures.</summary>
        [HttpGet("options")]
        [Authorize(Roles = "ChurchMember,AccountsOfficer,AccountsAdmin,Admin,FellowshipLeader,InventoryOfficer")]
        public async Task<IActionResult> Options()
        {
            var result = await _departmentService.GetActiveOptionsAsync();
            return result.Status ? Ok(result) : BadRequest(result);
        }

        [HttpGet]
        [Authorize(Roles = "AccountsOfficer,AccountsAdmin")]
        public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = true)
        {
            var result = await _departmentService.GetAllAsync(includeInactive);
            return result.Status ? Ok(result) : BadRequest(result);
        }

        [HttpGet("budgets")]
        [Authorize(Roles = "AccountsOfficer,AccountsAdmin")]
        public async Task<IActionResult> Budgets([FromQuery] int? year = null, [FromQuery] int? month = null, [FromQuery] bool includeInactive = false)
        {
            var result = await _departmentService.GetBudgetsAsync(year, month, includeInactive);
            return result.Status ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id}/budget")]
        [Authorize(Roles = "AccountsOfficer,AccountsAdmin")]
        public async Task<IActionResult> Budget(Guid id, [FromQuery] int? year = null, [FromQuery] int? month = null)
        {
            var result = await _departmentService.GetBudgetAsync(id, year, month);
            return result.Status ? Ok(result) : NotFound(result);
        }

        [HttpPost]
        [Authorize(Roles = "AccountsAdmin")]
        public async Task<IActionResult> Create([FromBody] CreateDepartmentModel model)
        {
            var result = await _departmentService.CreateAsync(model, CurrentUserId);
            return result.Status ? Ok(result) : BadRequest(result);
        }

        /// <summary>Edit details, budgets, or deactivate (IsActive = false). Departments are never deleted.</summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "AccountsAdmin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDepartmentModel model)
        {
            if (id != model.Id) return BadRequest(new { message = "Route id and body id must match.", status = false });
            var result = await _departmentService.UpdateAsync(model, CurrentUserId);
            return result.Status ? Ok(result) : BadRequest(result);
        }

        [HttpGet("legacy-names")]
        [Authorize(Roles = "AccountsAdmin")]
        public async Task<IActionResult> LegacyNames()
        {
            var result = await _departmentService.GetLegacyNamesAsync();
            return result.Status ? Ok(result) : BadRequest(result);
        }

        [HttpPost("link-legacy")]
        [Authorize(Roles = "AccountsAdmin")]
        public async Task<IActionResult> LinkLegacy([FromBody] LinkLegacyDepartmentModel model)
        {
            var result = await _departmentService.LinkLegacyAsync(model, CurrentUserId);
            return result.Status ? Ok(result) : BadRequest(result);
        }
    }
}
