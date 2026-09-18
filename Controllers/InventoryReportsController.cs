using ChurchPortal.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChurchPortal.Controllers
{
    [Route("api/Reports/Inventory")]
    [ApiController]
    [Authorize]
    public class InventoryReportsController : ControllerBase
    {
        private readonly IInventoryReportService _inventoryReportService;

        public InventoryReportsController(IInventoryReportService inventoryReportService)
        {
            _inventoryReportService = inventoryReportService;
        }

        [HttpGet("valuation")]
        public async Task<IActionResult> Valuation()
        {
            var result = await _inventoryReportService.GetValuationAsync();
            if (result.Status) return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("replacement-alerts")]
        public async Task<IActionResult> ReplacementAlerts([FromQuery] int years = 10)
        {
            var result = await _inventoryReportService.GetReplacementAlertsAsync(years);
            if (result.Status) return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("missing-verification")]
        public async Task<IActionResult> MissingVerification([FromQuery] int months = 6)
        {
            var result = await _inventoryReportService.GetMissingVerificationAsync(months);
            if (result.Status) return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("custodian-accountability")]
        public async Task<IActionResult> CustodianAccountability()
        {
            var result = await _inventoryReportService.GetCustodianAccountabilityAsync();
            if (result.Status) return Ok(result);
            return BadRequest(result);
        }
    }
}
