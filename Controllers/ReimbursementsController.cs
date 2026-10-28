using System.Security.Claims;
using ChurchPortal.Core.Dtos.Request;
using ChurchPortal.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChurchPortal.Controllers
{
    /// <summary>
    /// The class-level gate decides who can use reimbursements at all. Which stage
    /// action a person may take (forward, approve, pay, return, reject) is decided in
    /// ReimbursementService from the roles in their token, together with ownership and
    /// the current status. Doing it there means a refusal comes back as a 400 with a
    /// plain reason ("Only an Accounts Admin can confirm payment.") instead of a bare 403.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    // Every role except AttendanceOfficer can submit and track their own requests.
    // The Inventory Officer also uses this controller to verify physical items.
    // Seeing other people's requests and acting on them is limited to Accounts staff in the service.
    [Authorize(Roles = "ChurchMember,AccountsOfficer,AccountsAdmin,Admin,FellowshipLeader,InventoryOfficer")]
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
        [HttpPost("{id}/review")]
        public async Task<IActionResult> Review(Guid id, [FromBody] ReviewReimbursementModel? model)
        {
            var result = await _reimbursementService.ReviewAsync(id, model ?? new ReviewReimbursementModel(), CurrentUserId, CurrentRoles);
            if (result.Status) return Ok(result);
            return BadRequest(result);
        }

        /// <summary>Sends the request back to the member. Officer at Under Review, Admin at Pending Approval.</summary>
        [HttpPost("{id}/return")]
        public async Task<IActionResult> Return(Guid id, [FromBody] RequestReimbursementInfoModel model)
        {
            var result = await _reimbursementService.ReturnForCorrectionAsync(id, model, CurrentUserId, CurrentRoles);
            if (result.Status) return Ok(result);
            return BadRequest(result);
        }

        /// <summary>Older name for /return, kept so existing clients keep working.</summary>
        [HttpPost("{id}/request-info")]
        public Task<IActionResult> RequestInfo(Guid id, [FromBody] RequestReimbursementInfoModel model) => Return(id, model);

        [HttpPost("{id}/reject")]
        public async Task<IActionResult> Reject(Guid id, [FromBody] RejectReimbursementModel model)
        {
            var result = await _reimbursementService.RejectAsync(id, model, CurrentUserId, CurrentRoles);
            if (result.Status) return Ok(result);
            return BadRequest(result);
        }

        // ---------------- Stage three: Accounts Admin ----------------

        /// <summary>Pending Approval -> Approved. Does not mark the request as paid.</summary>
        [HttpPost("{id}/approve")]
        public async Task<IActionResult> Approve(Guid id, [FromBody] ReimbursementDecisionModel model)
        {
            var result = await _reimbursementService.ApproveAsync(id, model, CurrentUserId, CurrentRoles);
            if (result.Status) return Ok(result);
            return BadRequest(result);
        }

        /// <summary>Approved -> Paid. A separate, explicit confirmation after the money has been paid.</summary>
        [HttpPost("{id}/payment")]
        public async Task<IActionResult> RecordPayment(Guid id, [FromBody] RecordReimbursementPaymentModel model)
        {
            var result = await _reimbursementService.RecordPaymentAsync(id, model, CurrentUserId, CurrentRoles);
            if (result.Status) return Ok(result);
            return BadRequest(result);
        }

        // ---------------- Inventory Officer: physical items ----------------

        /// <summary>Physical items to verify. ?all=true includes items already verified or rejected.</summary>
        [HttpGet("verifications")]
        public async Task<IActionResult> Verifications([FromQuery] bool all = false)
        {
            var result = await _reimbursementService.GetVerificationQueueAsync(!all, CurrentUserId, CurrentRoles);
            if (result.Status) return Ok(result);
            return BadRequest(result);
        }

        /// <summary>Confirms a physical item and creates its inventory record.</summary>
        [HttpPost("items/{itemId}/verify")]
        public async Task<IActionResult> VerifyItem(Guid itemId, [FromBody] VerifyReimbursementItemModel model)
        {
            var result = await _reimbursementService.VerifyItemAsync(itemId, model, CurrentUserId, CurrentRoles);
            if (result.Status) return Ok(result);
            return BadRequest(result);
        }

        [HttpPost("items/{itemId}/reject")]
        public async Task<IActionResult> RejectItem(Guid itemId, [FromBody] RejectReimbursementItemModel model)
        {
            var result = await _reimbursementService.RejectItemAsync(itemId, model, CurrentUserId, CurrentRoles);
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
