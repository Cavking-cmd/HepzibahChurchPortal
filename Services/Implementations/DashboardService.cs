using ChurchPortal.Core.Dtos;
using ChurchPortal.Core.Dtos.Reports;
using ChurchPortal.Repositories.Interfaces;
using ChurchPortal.Services.Interfaces;

namespace ChurchPortal.Services.Implementations
{
    public class DashboardService : IDashboardService
    {
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly IServiceRepository _serviceRepository;
        private readonly IFellowshipCenterRepository _fellowshipCenterRepository;
        private readonly IInventoryItemRepository _inventoryItemRepository;
        private readonly IFellowshipReportService _fellowshipReportService;
        private readonly IInventoryReportService _inventoryReportService;

        public DashboardService(
            IAttendanceRepository attendanceRepository,
            IServiceRepository serviceRepository,
            IFellowshipCenterRepository fellowshipCenterRepository,
            IInventoryItemRepository inventoryItemRepository,
            IFellowshipReportService fellowshipReportService,
            IInventoryReportService inventoryReportService)
        {
            _attendanceRepository = attendanceRepository;
            _serviceRepository = serviceRepository;
            _fellowshipCenterRepository = fellowshipCenterRepository;
            _inventoryItemRepository = inventoryItemRepository;
            _fellowshipReportService = fellowshipReportService;
            _inventoryReportService = inventoryReportService;
        }

        public async Task<BaseResponse<DashboardSummaryDto>> GetSummaryAsync()
        {
            try
            {
                var attendances = await _attendanceRepository.GetAllAsync(a => !a.IsDeleted);
                var services = await _serviceRepository.GetAllAsync(s => !s.IsDeleted);
                var serviceMap = services.ToDictionary(s => s.Id);

                var now = DateTime.UtcNow;
                var currentMonthStart = new DateTime(now.Year, now.Month, 1);
                var lastMonthStart = currentMonthStart.AddMonths(-1);

                var joined = attendances
                    .Where(a => serviceMap.ContainsKey(a.ServiceId))
                    .Select(a => new { Attendance = a, Service = serviceMap[a.ServiceId] })
                    .ToList();

                var thisMonthTotal = joined
                    .Where(x => x.Service.Date >= currentMonthStart)
                    .Sum(x => x.Attendance.Total);

                var lastMonthRecords = joined
                    .Where(x => x.Service.Date >= lastMonthStart && x.Service.Date < currentMonthStart)
                    .ToList();
                var lastMonthTotal = lastMonthRecords.Sum(x => x.Attendance.Total);

                double? growthPercent = lastMonthRecords.Count == 0
                    ? null
                    : Math.Round(((double)(thisMonthTotal - lastMonthTotal) / lastMonthTotal) * 100.0, 1);

                var centers = await _fellowshipCenterRepository.GetAllAsync(c => !c.IsDeleted);
                var items = await _inventoryItemRepository.GetAllAsync(i => !i.IsDeleted);

                var alerts = new List<DashboardAlertDto>();

                var decliningCenters = await _fellowshipReportService.ComputeExpansionAlertsAsync();
                alerts.AddRange(decliningCenters.Select(c => new DashboardAlertDto
                {
                    Type = "FellowshipDeclining",
                    Message = $"{c.CenterName}: attendance declining for 3 months"
                }));

                var replacementItems = await _inventoryReportService.ComputeReplacementAlertsAsync(10);
                alerts.AddRange(replacementItems.Select(i => new DashboardAlertDto
                {
                    Type = "InventoryReplacement",
                    Message = $"{i.ItemName}: needs replacement"
                }));

                var result = new DashboardSummaryDto
                {
                    TotalAttendanceThisMonth = thisMonthTotal,
                    GrowthVsLastMonthPercent = growthPercent,
                    TotalActiveFellowshipCenters = centers.Count,
                    TotalAssetValue = items.Sum(i => i.Value * i.Quantity),
                    Alerts = alerts
                };

                return new BaseResponse<DashboardSummaryDto> { Message = "Dashboard summary generated.", Status = true, Data = result };
            }
            catch (Exception ex)
            {
                return new BaseResponse<DashboardSummaryDto> { Message = ex.Message, Status = false, Data = null };
            }
        }
    }
}
