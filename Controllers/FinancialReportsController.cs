using ChurchPortal.Core.Dtos.Reports;
using ChurchPortal.Core.Entities;
using ChurchPortal.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChurchPortal.Controllers
{
    /// <summary>
    /// Financial reports for Accounts staff. Members, pastors and other roles cannot call these.
    /// Common filters: from, to (yyyy-MM-dd), departmentId, classification (department|personal),
    /// status (enum number), payeeUserId.
    /// </summary>
    [Route("api/Reports/Finance")]
    [ApiController]
    [Authorize(Roles = "AccountsOfficer,AccountsAdmin")]
    public class FinancialReportsController : ControllerBase
    {
        private readonly IFinancialReportService _reports;

        public FinancialReportsController(IFinancialReportService reports)
        {
            _reports = reports;
        }

        private static FinancialReportFilter BuildFilter(
            DateTime? from, DateTime? to, Guid? departmentId, string? classification, int? status, Guid? payeeUserId)
        {
            var c = classification?.Trim().ToLowerInvariant();
            return new FinancialReportFilter
            {
                From = from.HasValue ? DateTime.SpecifyKind(from.Value.Date, DateTimeKind.Utc) : null,
                To = to.HasValue ? DateTime.SpecifyKind(to.Value.Date, DateTimeKind.Utc) : null,
                DepartmentId = departmentId,
                Classification = c is "department" or "personal" ? c : null,
                Status = status.HasValue && Enum.IsDefined(typeof(ReimbursementStatus), status.Value)
                    ? (ReimbursementStatus)status.Value
                    : null,
                PayeeUserId = payeeUserId
            };
        }

        [HttpGet("summary")]
        public async Task<IActionResult> Summary(
            [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] Guid? departmentId,
            [FromQuery] string? classification, [FromQuery] int? status, [FromQuery] Guid? payeeUserId)
        {
            var result = await _reports.GetSummaryAsync(BuildFilter(from, to, departmentId, classification, status, payeeUserId));
            return result.Status ? Ok(result) : BadRequest(result);
        }

        [HttpGet("monthly")]
        public async Task<IActionResult> Monthly(
            [FromQuery] int year, [FromQuery] int month, [FromQuery] Guid? departmentId,
            [FromQuery] string? classification, [FromQuery] Guid? payeeUserId)
        {
            var result = await _reports.GetMonthlyAsync(year, month, BuildFilter(null, null, departmentId, classification, null, payeeUserId));
            return result.Status ? Ok(result) : BadRequest(result);
        }

        [HttpGet("departments")]
        public async Task<IActionResult> Departments(
            [FromQuery] int year, [FromQuery] Guid? departmentId, [FromQuery] string? classification, [FromQuery] Guid? payeeUserId)
        {
            var result = await _reports.GetDepartmentSpendingAsync(year, BuildFilter(null, null, departmentId, classification, null, payeeUserId));
            return result.Status ? Ok(result) : BadRequest(result);
        }

        [HttpGet("trends")]
        public async Task<IActionResult> Trends(
            [FromQuery] int months = 12, [FromQuery] Guid? departmentId = null,
            [FromQuery] string? classification = null, [FromQuery] Guid? payeeUserId = null)
        {
            var result = await _reports.GetTrendsAsync(months, BuildFilter(null, null, departmentId, classification, null, payeeUserId));
            return result.Status ? Ok(result) : BadRequest(result);
        }

        [HttpGet("workflow")]
        public async Task<IActionResult> Workflow(
            [FromQuery] int staleDays = 14, [FromQuery] Guid? departmentId = null,
            [FromQuery] string? classification = null, [FromQuery] int? status = null, [FromQuery] Guid? payeeUserId = null)
        {
            var result = await _reports.GetWorkflowAsync(staleDays, BuildFilter(null, null, departmentId, classification, status, payeeUserId));
            return result.Status ? Ok(result) : BadRequest(result);
        }

        [HttpGet("forecast")]
        public async Task<IActionResult> Forecast(
            [FromQuery] int year, [FromQuery] Guid? departmentId = null, [FromQuery] string? classification = null)
        {
            var result = await _reports.GetForecastAsync(year, BuildFilter(null, null, departmentId, classification, null, null));
            return result.Status ? Ok(result) : BadRequest(result);
        }
    }
}
