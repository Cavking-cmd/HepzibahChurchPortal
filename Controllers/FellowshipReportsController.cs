using ChurchPortal.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChurchPortal.Controllers
{
    [Route("api/Reports/Fellowship")]
    [ApiController]
    [Authorize]
    public class FellowshipReportsController : ControllerBase
    {
        private readonly IFellowshipReportService _fellowshipReportService;

        public FellowshipReportsController(IFellowshipReportService fellowshipReportService)
        {
            _fellowshipReportService = fellowshipReportService;
        }

        [HttpGet("center-ranking")]
        public async Task<IActionResult> CenterRanking()
        {
            var result = await _fellowshipReportService.GetCenterRankingAsync();
            if (result.Status) return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("zone-summary")]
        public async Task<IActionResult> ZoneSummary()
        {
            var result = await _fellowshipReportService.GetZoneSummaryAsync();
            if (result.Status) return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("leader-trend")]
        public async Task<IActionResult> LeaderTrend()
        {
            var result = await _fellowshipReportService.GetLeaderTrendAsync();
            if (result.Status) return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("expansion-alerts")]
        public async Task<IActionResult> ExpansionAlerts()
        {
            var result = await _fellowshipReportService.GetExpansionAlertsAsync();
            if (result.Status) return Ok(result);
            return BadRequest(result);
        }
    }
}
