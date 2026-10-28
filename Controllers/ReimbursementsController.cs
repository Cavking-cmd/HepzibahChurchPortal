using System.Security.Claims;
using ChurchPortal.Core.Dtos.Request;
using ChurchPortal.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChurchPortal.Controllers
{
    /// <summary>
    /// Role gates here are the first check; the service checks roles, ownership and
    /// the current status again before changing anything.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "ChurchMember,AccountsOfficer,AccountsAdmin")]
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

        /// <summary>Budget position and projection for a request. Optional ?amount= to preview a different approved amount.</summary>
        [Authorize(Roles = "AccountsOfficer,AccountsAdmin")]
        [HttpGet("{id}/budget")]
        public async Task<IActionResult> Budget(Guid id, [FromQuery] decimal? amount = null)
        {
            var result = await _reimbursementService.GetBudgetAsync(id, amount, CurrentUserId, CurrentRoles);
            if (result.Status) return Ok(result);
            return BadRequest(result);
        }

        // ---------------- Member (own requests only) ----------------

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateReimbursementRequestModel model)
        {
            var result = await _reimbursementService.CreateAsync(model, CurrentUserId);
            if (result.Status) return Ok(result);
            return BadRequest(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateReimbursementRequestModel model)
        {
            if (id != model.Id) return BadRequest(new { message = "Route id and body id must match.", status = false });
            var result = await _reimbursementService.UpdateDraftAsync(model, CurrentUserId);
            if (result.Status) return Ok(result);
            return BadRequest(result);
        }

        [HttpPost("{id}/submit")]
        public async Task<IActionResult> Submit(Guid id)
        {
            var result = await _reimbursementService.SubmitAsync(id, CurrentUserId);
            if (result.Status) return Ok(result);
            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _reimbursementService.DeleteDraftAsync(id, CurrentUserId);
            if (result.Status) return NoContent();
            return BadRequest(result);
        }

        // ---------------- Stage two: Accounts Officer ----------------

        /// <summary>Under Review -> Pending Approval.</summary>
        [Authorize(Roles = "AccountsOfficer")]
        [HttpPost("{id}/review")]
        public async Task<IActionResult> Review(Guid id, [FromBody] ReviewReimbursementModel? model)
        {
            var result = await _reimbursementService.ReviewAsync(id, model ?? new ReviewReimbursementModel(), CurrentUserId, CurrentRoles);
            if (result.Status) return Ok(result);
            return BadRequest(result);
        }

        /// <summary>Sends the request back to the member. Officer at Under Review, Admin at Pending Approval.</summary>
        [Authorize(Roles = "AccountsOfficer,AccountsAdmin")]
        [HttpPost("{id}/return")]
        public async Task<IActionResult> Return(Guid id, [FromBody] RequestReimbursementInfoModel model)
        {
            var result = await _reimbursementService.ReturnForCorrectionAsync(id, model, CurrentUserId, CurrentRoles);
            if (result.Status) return Ok(result);
            return BadRequest(result);
        }

        /// <summary>Older name for /return, kept so existing clients keep working.</summary>
        [Authorize(Roles = "AccountsOfficer,AccountsAdmin")]
        [HttpPost("{id}/request-info")]
        public Task<IActionResult> RequestInfo(Guid id, [FromBody] RequestReimbursementInfoModel model) => Return(id, model);

        [Authorize(Roles = "AccountsOfficer,AccountsAdmin")]
        [HttpPost("{id}/reject")]
        public async Task<IActionResult> Reject(Guid id, [FromBody] RejectReimbursementModel model)
        {
            var result = await _reimbursementService.RejectAsync(id, model, CurrentUserId, CurrentRoles);
            if (result.Status) return Ok(result);
            return BadRequest(result);
        }

        // ---------------- Stage three: Accounts Admin ----------------

        /// <summary>Pending Approval -> Approved. Does not mark the request as paid.</summary>
        [Authorize(Roles = "AccountsAdmin")]
        [HttpPost("{id}/approve")]
        public async Task<IActionResult> Approve(Guid id, [FromBody] ReimbursementDecisionModel model)
        {
            var result = await _reimbursementService.ApproveAsync(id, model, CurrentUserId, CurrentRoles);
            if (result.Status) return Ok(result);
            return BadRequest(result);
        }

        /// <summary>Approved -> Paid. A separate, explicit confirmation after the money has been paid.</summary>
        [Authorize(Roles = "AccountsAdmin")]
        [HttpPost("{id}/payment")]
        public async Task<IActionResult> RecordPayment(Guid id, [FromBody] RecordReimbursementPaymentModel model)
        {
            var result = await _reimbursementService.RecordPaymentAsync(id, model, CurrentUserId, CurrentRoles);
            if (result.Status) return Ok(result);
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
