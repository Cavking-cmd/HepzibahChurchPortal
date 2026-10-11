using ChurchPortal.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

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

        private static DateTime? ParseMonth(string? month)
        {
            if (string.IsNullOrWhiteSpace(month)) return null;
            return DateTime.TryParseExact(month, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
                ? parsed
                : null;
        }

        [HttpGet("monthly-growth")]
        public async Task<IActionResult> MonthlyGrowth([FromQuery] int months = 12)
        {
            var result = await _attendanceReportService.GetMonthlyGrowthAsync(months);
            if (result.Status) return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("service-comparison")]
        public async Task<IActionResult> ServiceComparison([FromQuery] string? month = null)
        {
            var result = await _attendanceReportService.GetServiceComparisonAsync(ParseMonth(month));
            if (result.Status) return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("month-by-service")]
        public async Task<IActionResult> MonthByService([FromQuery] string? month = null)
        {
            var result = await _attendanceReportService.GetMonthByServiceAsync(ParseMonth(month) ?? DateTime.UtcNow);
            if (result.Status) return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("current-month-by-service")]
        public async Task<IActionResult> CurrentMonthByService()
        {
            var result = await _attendanceReportService.GetMonthByServiceAsync(DateTime.UtcNow);
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
        public async Task<IActionResult> FirstTimerConversion([FromQuery] string? month = null)
        {
            var result = await _attendanceReportService.GetFirstTimerConversionAsync(ParseMonth(month));
            if (result.Status) return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("preacher-impact")]
        public async Task<IActionResult> PreacherImpact([FromQuery] string? month = null)
        {
            var result = await _attendanceReportService.GetPreacherImpactAsync(ParseMonth(month));
            if (result.Status) return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("available-months")]
        public async Task<IActionResult> AvailableMonths()
        {
            var result = await _attendanceReportService.GetAvailableMonthsAsync();
            if (result.Status) return Ok(result);
            return BadRequest(result);
        }
    }
}
