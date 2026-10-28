using ChurchPortal.Core.Dtos;
using ChurchPortal.Core.Dtos.Reports;

namespace ChurchPortal.Services.Interfaces
{
    public interface IFinancialReportService
    {
        Task<BaseResponse<FinanceSummaryDto>> GetSummaryAsync(FinancialReportFilter filter);
        Task<BaseResponse<MonthlyExpenditureDto>> GetMonthlyAsync(int year, int month, FinancialReportFilter filter);
        Task<BaseResponse<DepartmentSpendingReportDto>> GetDepartmentSpendingAsync(int year, FinancialReportFilter filter);
        Task<BaseResponse<SpendingTrendsDto>> GetTrendsAsync(int months, FinancialReportFilter filter);
        Task<BaseResponse<WorkflowReportDto>> GetWorkflowAsync(int staleDays, FinancialReportFilter filter);
        Task<BaseResponse<FinanceForecastDto>> GetForecastAsync(int year, FinancialReportFilter filter);
    }
}
