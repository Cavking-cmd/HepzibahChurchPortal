using ChurchPortal.Core.Dtos;
using ChurchPortal.Core.Dtos.Reports;
using ChurchPortal.Core.Entities;
using ChurchPortal.Repositories.Interfaces;
using ChurchPortal.Services.Interfaces;

namespace ChurchPortal.Services.Implementations
{
    public class InventoryReportService : IInventoryReportService
    {
        private readonly IInventoryItemRepository _inventoryItemRepository;

        public InventoryReportService(IInventoryItemRepository inventoryItemRepository)
        {
            _inventoryItemRepository = inventoryItemRepository;
        }

        public async Task<BaseResponse<InventoryValuationDto>> GetValuationAsync()
        {
            try
            {
                var items = await _inventoryItemRepository.GetAllAsync(i => !i.IsDeleted);

                var byCategory = items
                    .GroupBy(i => i.Category)
                    .Select(g => new CategoryValuationDto
                    {
                        Category = g.Key,
                        TotalValue = g.Sum(i => i.Value * i.Quantity),
                        ItemCount = g.Count()
                    })
                    .ToList();

                var result = new InventoryValuationDto
                {
                    TotalValue = items.Sum(i => i.Value * i.Quantity),
                    ByCategory = byCategory
                };

                return new BaseResponse<InventoryValuationDto> { Message = "Inventory valuation report generated.", Status = true, Data = result };
            }
            catch (Exception ex)
            {
                return new BaseResponse<InventoryValuationDto> { Message = ex.Message, Status = false, Data = null };
            }
        }

        public async Task<List<ReplacementAlertDto>> ComputeReplacementAlertsAsync(int years)
        {
            if (years <= 0) years = 10;

            var items = await _inventoryItemRepository.GetAllAsync(i => !i.IsDeleted);
            var cutoff = DateTime.UtcNow.AddYears(-years);
            var now = DateTime.UtcNow;

            var result = new List<ReplacementAlertDto>();
            foreach (var item in items)
            {
                bool conditionFlag = item.Condition == ItemCondition.Replace;
                bool ageFlag = item.PurchaseDate < cutoff;

                if (!conditionFlag && !ageFlag) continue;

                var ageInYears = Math.Round((now - item.PurchaseDate).TotalDays / 365.25, 1);
                var reason = conditionFlag ? "Condition marked Replace" : $"Older than {years} years";

                result.Add(new ReplacementAlertDto
                {
                    ItemId = item.Id,
                    ItemName = item.ItemName,
                    Category = item.Category,
                    Condition = item.Condition.ToString(),
                    PurchaseDate = item.PurchaseDate,
                    AgeInYears = ageInYears,
                    Reason = reason
                });
            }

            return result;
        }

        public async Task<BaseResponse<List<ReplacementAlertDto>>> GetReplacementAlertsAsync(int years)
        {
            try
            {
                var result = await ComputeReplacementAlertsAsync(years);
                return new BaseResponse<List<ReplacementAlertDto>> { Message = "Replacement alerts generated.", Status = true, Data = result };
            }
            catch (Exception ex)
            {
                return new BaseResponse<List<ReplacementAlertDto>> { Message = ex.Message, Status = false, Data = null };
            }
        }

        public async Task<BaseResponse<List<MissingVerificationDto>>> GetMissingVerificationAsync(int months)
        {
            try
            {
                if (months <= 0) months = 6;

                var items = await _inventoryItemRepository.GetAllAsync(i => !i.IsDeleted);
                var now = DateTime.UtcNow;
                var cutoff = now.AddMonths(-months);

                var result = items
                    .Where(i => i.LastVerifiedDate == null || i.LastVerifiedDate < cutoff)
                    .Select(i => new MissingVerificationDto
                    {
                        ItemId = i.Id,
                        ItemName = i.ItemName,
                        Category = i.Category,
                        LastVerifiedDate = i.LastVerifiedDate,
                        MonthsSinceVerified = i.LastVerifiedDate.HasValue
                            ? Math.Round((now - i.LastVerifiedDate.Value).TotalDays / 30.44, 1)
                            : (double?)null
                    })
                    .ToList();

                return new BaseResponse<List<MissingVerificationDto>> { Message = "Missing verification report generated.", Status = true, Data = result };
            }
            catch (Exception ex)
            {
                return new BaseResponse<List<MissingVerificationDto>> { Message = ex.Message, Status = false, Data = null };
            }
        }

        public async Task<BaseResponse<List<CustodianAccountabilityDto>>> GetCustodianAccountabilityAsync()
        {
            try
            {
                var items = await _inventoryItemRepository.GetAllAsync(i => !i.IsDeleted);

                var result = items
                    .GroupBy(i => i.Custodian)
                    .Select(g => new CustodianAccountabilityDto
                    {
                        Custodian = g.Key,
                        ItemCount = g.Count(),
                        TotalValue = g.Sum(i => i.Value * i.Quantity),
                        Items = g.Select(i => new CustodianItemDto { ItemId = i.Id, ItemName = i.ItemName, Value = i.Value }).ToList()
                    })
                    .ToList();

                return new BaseResponse<List<CustodianAccountabilityDto>> { Message = "Custodian accountability report generated.", Status = true, Data = result };
            }
            catch (Exception ex)
            {
                return new BaseResponse<List<CustodianAccountabilityDto>> { Message = ex.Message, Status = false, Data = null };
            }
        }
    }
}
