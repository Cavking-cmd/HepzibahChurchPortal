using System.Security.Claims;
using ChurchPortal.Core.Dtos.Request;
using ChurchPortal.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChurchPortal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "ChurchMember,AccountsOfficer,AccountsAdmin,Pastor")]
    public class ReimbursementsController : ControllerBase
    {
        private readonly IReimbursementService _reimbursementService;

        public ReimbursementsController(IReimbursementService reimbursementService)
        {
            _reimbursementService = reimbursementService;
        }

        private Guid CurrentUserId =>
            Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : Guid.Empty;

        private IReadOnlyCollection<string> CurrentRoles =>
            User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _reimbursementService.GetAllAsync(CurrentUserId, CurrentRoles);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var result = await _reimbursementService.GetAsync(id, CurrentUserId, CurrentRoles);
            if (result.Status) return Ok(result);
            return NotFound(result);
        }

        [Authorize(Roles = "ChurchMember,AccountsAdmin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateReimbursementRequestModel model)
        {
            var result = await _reimbursementService.CreateAsync(model, CurrentUserId);
            if (result.Status) return Ok(result);
            return BadRequest(result);
        }

        [Authorize(Roles = "ChurchMember,AccountsAdmin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateReimbursementRequestModel model)
        {
            if (id != model.Id) return BadRequest("Route id and body id must match.");
            var result = await _reimbursementService.UpdateDraftAsync(model, CurrentUserId);
            if (result.Status) return Ok(result);
            return BadRequest(result);
        }

        [Authorize(Roles = "ChurchMember,AccountsAdmin")]
        [HttpPost("{id}/submit")]
        public async Task<IActionResult> Submit(Guid id)
        {
            var result = await _reimbursementService.SubmitAsync(id, CurrentUserId);
            if (result.Status) return Ok(result);
            return BadRequest(result);
        }

        [Authorize(Roles = "Pastor,AccountsAdmin")]
        [HttpPost("{id}/minister-approve")]
        public async Task<IActionResult> MinisterApprove(Guid id, [FromBody] ReimbursementDecisionModel model)
        {
            var result = await _reimbursementService.MinisterApproveAsync(id, model, CurrentUserId);
            if (result.Status) return Ok(result);
            return BadRequest(result);
        }

        [Authorize(Roles = "AccountsOfficer,AccountsAdmin")]
        [HttpPost("{id}/request-info")]
        public async Task<IActionResult> RequestInfo(Guid id, [FromBody] RequestReimbursementInfoModel model)
        {
            var result = await _reimbursementService.RequestInfoAsync(id, model, CurrentUserId);
            if (result.Status) return Ok(result);
            return BadRequest(result);
        }

        [Authorize(Roles = "AccountsOfficer,AccountsAdmin,Pastor")]
        [HttpPost("{id}/reject")]
        public async Task<IActionResult> Reject(Guid id, [FromBody] RejectReimbursementModel model)
        {
            var result = await _reimbursementService.RejectAsync(id, model, CurrentUserId);
            if (result.Status) return Ok(result);
            return BadRequest(result);
        }

        [HttpPost("{id}/approve")]
        public async Task<IActionResult> Approve(Guid id, [FromBody] ReimbursementDecisionModel model)
        {
            var roles = CurrentRoles;
            if (!roles.Contains("AccountsOfficer") && !roles.Contains("AccountsAdmin"))
            {
                return Forbid();
            }
            var result = await _reimbursementService.ApproveAsync(id, model, CurrentUserId);
            if (result.Status) return Ok(result);
            return BadRequest(result);
        }

        [HttpPost("{id}/payment")]
        public async Task<IActionResult> RecordPayment(Guid id, [FromBody] RecordReimbursementPaymentModel model)
        {
            var roles = CurrentRoles;
            if (!roles.Contains("AccountsOfficer") && !roles.Contains("AccountsAdmin"))
            {
                return Forbid();
            }
            var result = await _reimbursementService.RecordPaymentAsync(id, model, CurrentUserId);
            if (result.Status) return Ok(result);
            return BadRequest(result);
        }

        [Authorize(Roles = "ChurchMember,AccountsAdmin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _reimbursementService.DeleteDraftAsync(id, CurrentUserId);
            if (result.Status) return NoContent();
            return BadRequest(result);
        }

        [HttpGet("receipts/{receiptId}")]
        public async Task<IActionResult> GetReceipt(Guid receiptId)
        {
            var result = await _reimbursementService.GetReceiptAsync(receiptId, CurrentUserId, CurrentRoles);
            if (result.Status) return Ok(result);
            return NotFound(result);
        }
    }
}
