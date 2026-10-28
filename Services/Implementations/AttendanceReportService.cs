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

        private static bool InMonth(Service service, DateTime month) =>
            service.Date.Year == month.Year && service.Date.Month == month.Month;

        public async Task<BaseResponse<ServiceComparisonReportDto>> GetServiceComparisonAsync(DateTime? month = null)
        {
            try
            {
                var joined = await GetJoinedAsync();

                if (month.HasValue && month.Value != default)
                    joined = joined.Where(x => InMonth(x.Service, month.Value)).ToList();

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

                var onlineByMonth = joined
                    .GroupBy(x => new { x.Service.Date.Year, x.Service.Date.Month })
                    .Select(g => new { g.Key.Year, g.Key.Month, Online = g.Sum(x => x.Attendance.OnlineAttendance) })
                    .ToList();

                var physicalByMonth = joined
                    .GroupBy(x => new { x.Service.Date.Year, x.Service.Date.Month })
                    .Select(g => new { g.Key.Year, g.Key.Month, Physical = g.Sum(x => x.Attendance.Men + x.Attendance.Women + x.Attendance.Children) })
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

        private static string ShortServiceType(ServiceType type) => type switch
        {
            ServiceType.SundayService => "Sunday",
            ServiceType.WednesdayBibleStudy => "Bible Study",
            ServiceType.Rodah => "Rodah",
            ServiceType.Special => "Special",
            _ => type.ToString()
        };

        public async Task<BaseResponse<MonthServiceAttendanceDto>> GetMonthByServiceAsync(DateTime month)
        {
            try
            {
                var monthServices = await _serviceRepository.GetAllAsync(s => !s.IsDeleted && s.Date.Year == month.Year && s.Date.Month == month.Month);
                var attendances = await _attendanceRepository.GetAllAsync(a => !a.IsDeleted);

                var attendanceByService = attendances
                    .GroupBy(a => a.ServiceId)
                    .ToDictionary(g => g.Key, g => new
                    {
                        Online = g.Sum(a => a.OnlineAttendance),
                        Physical = g.Sum(a => a.Men + a.Women + a.Children)
                    });

                var services = monthServices
                    .OrderBy(s => s.Date)
                    .Select(s =>
                    {
                        attendanceByService.TryGetValue(s.Id, out var agg);
                        var online = agg?.Online ?? 0;
                        var physical = agg?.Physical ?? 0;
                        return new ServiceMonthAttendanceDto
                        {
                            ServiceId = s.Id,
                            Date = s.Date,
                            ServiceType = s.ServiceType.ToString(),
                            Label = $"{s.Date:dd MMM} · {ShortServiceType(s.ServiceType)}",
                            Online = online,
                            Physical = physical,
                            Total = physical + online
                        };
                    })
                    .ToList();

                var result = new MonthServiceAttendanceDto
                {
                    Year = month.Year,
                    Month = month.Month,
                    MonthLabel = new DateTime(month.Year, month.Month, 1).ToString("MMMM yyyy"),
                    Services = services
                };

                return new BaseResponse<MonthServiceAttendanceDto> { Message = "Month service attendance report generated.", Status = true, Data = result };
            }
            catch (Exception ex)
            {
                return new BaseResponse<MonthServiceAttendanceDto> { Message = ex.Message, Status = false, Data = null };
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

        public async Task<BaseResponse<FirstTimerConversionDto>> GetFirstTimerConversionAsync(DateTime? month = null)
        {
            try
            {
                var joined = await GetJoinedAsync();

                if (month.HasValue && month.Value != default)
                    joined = joined.Where(x => InMonth(x.Service, month.Value)).ToList();

                var totalFirstTimers = joined.Sum(x => x.Attendance.FirstTimers);
                var totalNewConverts = joined.Sum(x => x.Attendance.NewConverts);

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

        public async Task<BaseResponse<List<PreacherImpactDto>>> GetPreacherImpactAsync(DateTime? month = null)
        {
            try
            {
                var joined = await GetJoinedAsync();

                if (month.HasValue && month.Value != default)
                    joined = joined.Where(x => InMonth(x.Service, month.Value)).ToList();

                // Group by pastor record when linked, so spelling variations no longer split
                // one person's figures. Older unlinked services fall back to their typed name.
                var result = joined
                    .Where(x => x.Service.PastorId != null || !string.IsNullOrWhiteSpace(x.Service.Preacher))
                    .GroupBy(x => x.Service.PastorId?.ToString() ?? "text:" + NameNormalizer.Normalize(x.Service.Preacher))
                    .Select(g => new PreacherImpactDto
                    {
                        Preacher = g.Select(x => x.Service.Preacher).FirstOrDefault(p => !string.IsNullOrWhiteSpace(p)) ?? "Unknown",
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

        public async Task<BaseResponse<List<MonthOptionDto>>> GetAvailableMonthsAsync()
        {
            try
            {
                var services = await _serviceRepository.GetAllAsync(s => !s.IsDeleted);

                var months = services
                    .Select(s => new { s.Date.Year, s.Date.Month })
                    .Distinct()
                    .OrderBy(m => m.Year).ThenBy(m => m.Month)
                    .Select(m => new MonthOptionDto
                    {
                        Year = m.Year,
                        Month = m.Month,
                        Label = new DateTime(m.Year, m.Month, 1).ToString("MMMM yyyy")
                    })
                    .ToList();

                return new BaseResponse<List<MonthOptionDto>> { Message = "Available report months retrieved.", Status = true, Data = months };
            }
            catch (Exception ex)
            {
                return new BaseResponse<List<MonthOptionDto>> { Message = ex.Message, Status = false, Data = null };
            }
        }
    }
}
