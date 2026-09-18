using ChurchPortal.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChurchPortal.Controllers
{
    [Route("api/Reports/Attendance")]
    [ApiController]
    [Authorize]
    public class AttendanceReportsController : ControllerBase
    {
        private readonly IAttendanceReportService _attendanceReportService;

        public AttendanceReportsController(IAttendanceReportService attendanceReportService)
        {
            _attendanceReportService = attendanceReportService;
        }

        [HttpGet("monthly-growth")]
        public async Task<IActionResult> MonthlyGrowth([FromQuery] int months = 12)
        {
            var result = await _attendanceReportService.GetMonthlyGrowthAsync(months);
            if (result.Status) return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("service-comparison")]
        public async Task<IActionResult> ServiceComparison()
        {
            var result = await _attendanceReportService.GetServiceComparisonAsync();
            if (result.Status) return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("demographics")]
        public async Task<IActionResult> Demographics([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var result = await _attendanceReportService.GetDemographicsAsync(startDate, endDate);
            if (result.Status) return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("first-timer-conversion")]
        public async Task<IActionResult> FirstTimerConversion()
        {
            var result = await _attendanceReportService.GetFirstTimerConversionAsync();
            if (result.Status) return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("preacher-impact")]
        public async Task<IActionResult> PreacherImpact()
        {
            var result = await _attendanceReportService.GetPreacherImpactAsync();
            if (result.Status) return Ok(result);
            return BadRequest(result);
        }
    }
}
