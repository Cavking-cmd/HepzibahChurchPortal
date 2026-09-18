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
    public class InventoryItemController : ControllerBase
    {
        private readonly IInventoryItemService _inventoryItemService;

        public InventoryItemController(IInventoryItemService inventoryItemService)
        {
            _inventoryItemService = inventoryItemService;
        }

        private Guid CurrentUserId =>
            Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : Guid.Empty;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _inventoryItemService.GetAll();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var result = await _inventoryItemService.GetAsync(id);
            if (result.Status) return Ok(result);
            return NotFound(result);
        }

        [Authorize(Roles = "Admin,InventoryOfficer")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateInventoryItemRequestModel model)
        {
            var result = await _inventoryItemService.CreateAsync(model, CurrentUserId);
            if (result.Status) return Ok(result);
            return BadRequest(result);
        }

        [Authorize(Roles = "Admin,InventoryOfficer")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateInventoryItemModel model)
        {
            if (id != model.Id) return BadRequest("Route id and body id must match.");
            var result = await _inventoryItemService.UpdateAsync(model, CurrentUserId);
            if (result.Status) return Ok(result);
            return BadRequest(result);
        }

        [Authorize(Roles = "Admin,InventoryOfficer")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _inventoryItemService.SoftDeleteAsync(id, CurrentUserId);
            if (result.Status) return NoContent();
            return NotFound(result);
        }
    }
}
