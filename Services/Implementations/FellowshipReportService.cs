using ChurchPortal.Core.Dtos;
using ChurchPortal.Core.Dtos.Reports;
using ChurchPortal.Core.Entities;
using ChurchPortal.Repositories.Interfaces;
using ChurchPortal.Services.Interfaces;

namespace ChurchPortal.Services.Implementations
{
    public class FellowshipReportService : IFellowshipReportService
    {
        private readonly IFellowshipAttendanceRepository _fellowshipAttendanceRepository;
        private readonly IFellowshipCenterRepository _fellowshipCenterRepository;

        public FellowshipReportService(
            IFellowshipAttendanceRepository fellowshipAttendanceRepository,
            IFellowshipCenterRepository fellowshipCenterRepository)
        {
            _fellowshipAttendanceRepository = fellowshipAttendanceRepository;
            _fellowshipCenterRepository = fellowshipCenterRepository;
        }

        private async Task<(List<FellowshipAttendance> Attendances, List<FellowshipCenter> Centers)> GetDataAsync()
        {
            var attendances = await _fellowshipAttendanceRepository.GetAllAsync(a => !a.IsDeleted);
            var centers = await _fellowshipCenterRepository.GetAllAsync(c => !c.IsDeleted);
            return (attendances, centers);
        }

        public async Task<BaseResponse<List<CenterRankingDto>>> GetCenterRankingAsync()
        {
            try
            {
                var (attendances, centers) = await GetDataAsync();
                var centerMap = centers.ToDictionary(c => c.Id);

                var result = attendances
                    .Where(a => centerMap.ContainsKey(a.FellowshipCenterId))
                    .GroupBy(a => a.FellowshipCenterId)
                    .Select(g =>
                    {
                        var center = centerMap[g.Key];
                        return new CenterRankingDto
                        {
                            CenterId = center.Id,
                            CenterName = center.CenterName,
                            Zone = center.Zone,
                            TotalAttendance = g.Sum(a => a.Total),
                            AverageAttendance = g.Average(a => a.Total),
                            RecordCount = g.Count()
                        };
                    })
                    .OrderByDescending(x => x.TotalAttendance)
                    .ToList();

                return new BaseResponse<List<CenterRankingDto>> { Message = "Center ranking report generated.", Status = true, Data = result };
            }
            catch (Exception ex)
            {
                return new BaseResponse<List<CenterRankingDto>> { Message = ex.Message, Status = false, Data = null };
            }
        }

        public async Task<BaseResponse<List<ZoneSummaryDto>>> GetZoneSummaryAsync()
        {
            try
            {
                var (attendances, centers) = await GetDataAsync();
                var centerMap = centers.ToDictionary(c => c.Id);

                var result = centers
                    .GroupBy(c => c.Zone)
                    .Select(g =>
                    {
                        var centerIds = g.Select(c => c.Id).ToHashSet();
                        var zoneAttendances = attendances.Where(a => centerIds.Contains(a.FellowshipCenterId)).ToList();
                        var total = zoneAttendances.Sum(a => a.Total);
                        return new ZoneSummaryDto
                        {
                            Zone = g.Key,
                            TotalAttendance = total,
                            CenterCount = g.Count(),
                            AverageAttendance = zoneAttendances.Count > 0 ? zoneAttendances.Average(a => a.Total) : 0
                        };
                    })
                    .ToList();

                return new BaseResponse<List<ZoneSummaryDto>> { Message = "Zone summary report generated.", Status = true, Data = result };
            }
            catch (Exception ex)
            {
                return new BaseResponse<List<ZoneSummaryDto>> { Message = ex.Message, Status = false, Data = null };
            }
        }

        public async Task<BaseResponse<List<LeaderTrendDto>>> GetLeaderTrendAsync()
        {
            try
            {
                var (attendances, centers) = await GetDataAsync();
                var centerMap = centers.ToDictionary(c => c.Id);

                var result = attendances
                    .Where(a => centerMap.ContainsKey(a.FellowshipCenterId))
                    .GroupBy(a => centerMap[a.FellowshipCenterId].LeaderName)
                    .Select(g =>
                    {
                        var firstCenterId = g.First().FellowshipCenterId;
                        return new LeaderTrendDto
                        {
                            LeaderName = g.Key,
                            CenterName = centerMap[firstCenterId].CenterName,
                            AverageAttendance = g.Average(a => a.Total),
                            RecordCount = g.Count()
                        };
                    })
                    .ToList();

                return new BaseResponse<List<LeaderTrendDto>> { Message = "Leader trend report generated.", Status = true, Data = result };
            }
            catch (Exception ex)
            {
                return new BaseResponse<List<LeaderTrendDto>> { Message = ex.Message, Status = false, Data = null };
            }
        }

        public async Task<List<ExpansionAlertDto>> ComputeExpansionAlertsAsync()
        {
            var (attendances, centers) = await GetDataAsync();
            var centerMap = centers.ToDictionary(c => c.Id);

            var alerts = new List<ExpansionAlertDto>();

            var byCenter = attendances
                .Where(a => centerMap.ContainsKey(a.FellowshipCenterId))
                .GroupBy(a => a.FellowshipCenterId);

            foreach (var g in byCenter)
            {
                var monthlyTotals = g
                    .GroupBy(a => new { a.Date.Year, a.Date.Month })
                    .Select(mg => new { mg.Key.Year, mg.Key.Month, Total = mg.Sum(a => a.Total) })
                    .OrderByDescending(m => m.Year).ThenByDescending(m => m.Month)
                    .Take(3)
                    .OrderBy(m => m.Year).ThenBy(m => m.Month)
                    .ToList();

                if (monthlyTotals.Count < 3) continue;

                bool strictlyDecreasing = monthlyTotals[0].Total > monthlyTotals[1].Total && monthlyTotals[1].Total > monthlyTotals[2].Total;
                if (!strictlyDecreasing) continue;

                var center = centerMap[g.Key];
                alerts.Add(new ExpansionAlertDto
                {
                    CenterId = center.Id,
                    CenterName = center.CenterName,
                    AlertType = "Declining",
                    Detail = $"Attendance fell from {monthlyTotals[0].Total} to {monthlyTotals[1].Total} to {monthlyTotals[2].Total} over the last 3 months"
                });
            }

            return alerts;
        }

        public async Task<BaseResponse<List<ExpansionAlertDto>>> GetExpansionAlertsAsync()
        {
            try
            {
                var alerts = await ComputeExpansionAlertsAsync();
                return new BaseResponse<List<ExpansionAlertDto>> { Message = "Expansion alerts generated.", Status = true, Data = alerts };
            }
            catch (Exception ex)
            {
                return new BaseResponse<List<ExpansionAlertDto>> { Message = ex.Message, Status = false, Data = null };
            }
        }
    }
}
