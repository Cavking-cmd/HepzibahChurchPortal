using ChurchPortal.Core.Dtos;
using ChurchPortal.Core.Dtos.Reports;
using ChurchPortal.Core.Entities;
using ChurchPortal.Repositories.Interfaces;
using ChurchPortal.Services.Interfaces;

namespace ChurchPortal.Services.Implementations
{
    public class AttendanceReportService : IAttendanceReportService
    {
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly IServiceRepository _serviceRepository;

        public AttendanceReportService(IAttendanceRepository attendanceRepository, IServiceRepository serviceRepository)
        {
            _attendanceRepository = attendanceRepository;
            _serviceRepository = serviceRepository;
        }

        private async Task<List<(Attendance Attendance, Service Service)>> GetJoinedAsync()
        {
            var attendances = await _attendanceRepository.GetAllAsync(a => !a.IsDeleted);
            var services = await _serviceRepository.GetAllAsync(s => !s.IsDeleted);
            var serviceMap = services.ToDictionary(s => s.Id);

            var joined = new List<(Attendance, Service)>();
            foreach (var a in attendances)
            {
                if (serviceMap.TryGetValue(a.ServiceId, out var service))
                {
                    joined.Add((a, service));
                }
            }
            return joined;
        }

        public async Task<BaseResponse<List<MonthlyGrowthDto>>> GetMonthlyGrowthAsync(int months)
        {
            try
            {
                if (months <= 0) months = 12;

                var joined = await GetJoinedAsync();

                var now = DateTime.UtcNow;
                var startMonth = new DateTime(now.Year, now.Month, 1).AddMonths(-(months - 1));

                var grouped = joined
                    .Where(x => x.Service.Date >= startMonth)
                    .GroupBy(x => new { x.Service.Date.Year, x.Service.Date.Month })
                    .Select(g => new
                    {
                        g.Key.Year,
                        g.Key.Month,
                        Total = g.Sum(x => x.Attendance.Total)
                    })
                    .OrderBy(g => g.Year).ThenBy(g => g.Month)
                    .ToList();

                var result = new List<MonthlyGrowthDto>();
                int? previousTotal = null;
                foreach (var g in grouped)
                {
                    double? percentChange = null;
                    if (previousTotal.HasValue)
                    {
                        percentChange = previousTotal.Value == 0
                            ? (g.Total > 0 ? 100.0 : 0.0)
                            : Math.Round(((double)(g.Total - previousTotal.Value) / previousTotal.Value) * 100.0, 1);
                    }

                    result.Add(new MonthlyGrowthDto
                    {
                        Year = g.Year,
                        Month = g.Month,
                        MonthLabel = new DateTime(g.Year, g.Month, 1).ToString("MMM yyyy"),
                        TotalAttendance = g.Total,
                        PercentChangeFromPreviousMonth = percentChange
                    });

                    previousTotal = g.Total;
                }

                return new BaseResponse<List<MonthlyGrowthDto>> { Message = "Monthly growth report generated.", Status = true, Data = result };
            }
            catch (Exception ex)
            {
                return new BaseResponse<List<MonthlyGrowthDto>> { Message = ex.Message, Status = false, Data = null };
            }
        }

        public async Task<BaseResponse<ServiceComparisonReportDto>> GetServiceComparisonAsync()
        {
            try
            {
                var joined = await GetJoinedAsync();
                var services = await _serviceRepository.GetAllAsync(s => !s.IsDeleted);

                var byServiceType = joined
                    .GroupBy(x => x.Service.ServiceType)
                    .Select(g => new ServiceTypeComparisonDto
                    {
                        ServiceType = g.Key.ToString(),
                        TotalAttendance = g.Sum(x => x.Attendance.Total),
                        AverageAttendance = g.Average(x => x.Attendance.Total),
                        RecordCount = g.Count()
                    })
                    .ToList();

                var onlineByMonth = services
                    .GroupBy(s => new { s.Date.Year, s.Date.Month })
                    .Select(g => new { g.Key.Year, g.Key.Month, Online = g.Sum(s => s.OnlineAttendance) })
                    .ToList();

                var physicalByMonth = joined
                    .GroupBy(x => new { x.Service.Date.Year, x.Service.Date.Month })
                    .Select(g => new { g.Key.Year, g.Key.Month, Physical = g.Sum(x => x.Attendance.Total) })
                    .ToList();

                var months = onlineByMonth.Select(x => (x.Year, x.Month))
                    .Union(physicalByMonth.Select(x => (x.Year, x.Month)))
                    .OrderBy(x => x.Year).ThenBy(x => x.Month)
                    .ToList();

                var onlineVsPhysical = months.Select(m => new OnlineVsPhysicalDto
                {
                    MonthLabel = new DateTime(m.Year, m.Month, 1).ToString("MMM yyyy"),
                    TotalOnline = onlineByMonth.FirstOrDefault(x => x.Year == m.Year && x.Month == m.Month)?.Online ?? 0,
                    TotalPhysical = physicalByMonth.FirstOrDefault(x => x.Year == m.Year && x.Month == m.Month)?.Physical ?? 0
                }).ToList();

                var result = new ServiceComparisonReportDto
                {
                    ByServiceType = byServiceType,
                    OnlineVsPhysicalByMonth = onlineVsPhysical
                };

                return new BaseResponse<ServiceComparisonReportDto> { Message = "Service comparison report generated.", Status = true, Data = result };
            }
            catch (Exception ex)
            {
                return new BaseResponse<ServiceComparisonReportDto> { Message = ex.Message, Status = false, Data = null };
            }
        }

        public async Task<BaseResponse<DemographicsReportDto>> GetDemographicsAsync(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var joined = await GetJoinedAsync();

                var filtered = joined.AsEnumerable();
                if (startDate.HasValue) filtered = filtered.Where(x => x.Service.Date >= startDate.Value);
                if (endDate.HasValue) filtered = filtered.Where(x => x.Service.Date <= endDate.Value);

                var list = filtered.ToList();

                var result = new DemographicsReportDto
                {
                    TotalMen = list.Sum(x => x.Attendance.Men),
                    TotalWomen = list.Sum(x => x.Attendance.Women),
                    TotalChildren = list.Sum(x => x.Attendance.Children)
                };

                return new BaseResponse<DemographicsReportDto> { Message = "Demographics report generated.", Status = true, Data = result };
            }
            catch (Exception ex)
            {
                return new BaseResponse<DemographicsReportDto> { Message = ex.Message, Status = false, Data = null };
            }
        }

        public async Task<BaseResponse<FirstTimerConversionDto>> GetFirstTimerConversionAsync()
        {
            try
            {
                var attendances = await _attendanceRepository.GetAllAsync(a => !a.IsDeleted);

                var totalFirstTimers = attendances.Sum(a => a.FirstTimers);
                var totalNewConverts = attendances.Sum(a => a.NewConverts);

                var rate = totalFirstTimers == 0 ? 0.0 : Math.Round((double)totalNewConverts / totalFirstTimers * 100.0, 1);

                var result = new FirstTimerConversionDto
                {
                    TotalFirstTimers = totalFirstTimers,
                    TotalNewConverts = totalNewConverts,
                    ConversionRatePercent = rate
                };

                return new BaseResponse<FirstTimerConversionDto> { Message = "First-timer conversion report generated.", Status = true, Data = result };
            }
            catch (Exception ex)
            {
                return new BaseResponse<FirstTimerConversionDto> { Message = ex.Message, Status = false, Data = null };
            }
        }

        public async Task<BaseResponse<List<PreacherImpactDto>>> GetPreacherImpactAsync()
        {
            try
            {
                var joined = await GetJoinedAsync();

                var result = joined
                    .Where(x => !string.IsNullOrWhiteSpace(x.Service.Preacher))
                    .GroupBy(x => x.Service.Preacher!)
                    .Select(g => new PreacherImpactDto
                    {
                        Preacher = g.Key,
                        TotalAttendance = g.Sum(x => x.Attendance.Total),
                        AverageAttendance = g.Average(x => x.Attendance.Total),
                        ServiceCount = g.Count()
                    })
                    .OrderByDescending(x => x.TotalAttendance)
                    .ToList();

                return new BaseResponse<List<PreacherImpactDto>> { Message = "Preacher impact report generated.", Status = true, Data = result };
            }
            catch (Exception ex)
            {
                return new BaseResponse<List<PreacherImpactDto>> { Message = ex.Message, Status = false, Data = null };
            }
        }
    }
}
