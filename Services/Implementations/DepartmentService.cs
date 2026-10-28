using ChurchPortal.Core.Dtos;
using ChurchPortal.Core.Dtos.Request;
using ChurchPortal.Core.Entities;
using ChurchPortal.Repositories.Interfaces;
using ChurchPortal.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ChurchPortal.Services.Implementations
{
    public class DepartmentService : IDepartmentService
    {
        private const int MaxNameLength = 120;
        private const decimal MaxBudget = 999_999_999_999m;

        private readonly IDepartmentRepository _departmentRepository;
        private readonly IReimbursementRepository _reimbursementRepository;
        private readonly IUserRepository _userRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IBudgetService _budgetService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DepartmentService> _logger;

        public DepartmentService(
            IDepartmentRepository departmentRepository,
            IReimbursementRepository reimbursementRepository,
            IUserRepository userRepository,
            IAuditLogRepository auditLogRepository,
            IBudgetService budgetService,
            IUnitOfWork unitOfWork,
            ILogger<DepartmentService> logger)
        {
            _departmentRepository = departmentRepository;
            _reimbursementRepository = reimbursementRepository;
            _userRepository = userRepository;
            _auditLogRepository = auditLogRepository;
            _budgetService = budgetService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<BaseResponse<List<DepartmentOptionDto>>> GetActiveOptionsAsync()
        {
            try
            {
                var departments = await _departmentRepository.GetAllOrderedAsync(activeOnly: true);
                var data = departments.Select(d => new DepartmentOptionDto { Id = d.Id, Name = d.Name }).ToList();
                return Ok(data, "Departments found.");
            }
            catch (Exception ex)
            {
                return Unexpected<List<DepartmentOptionDto>>(ex, "load departments");
            }
        }

        public async Task<BaseResponse<List<DepartmentDto>>> GetAllAsync(bool includeInactive)
        {
            try
            {
                var departments = await _departmentRepository.GetAllOrderedAsync(activeOnly: !includeInactive);
                var counts = await _departmentRepository.GetRequestCountsAsync();
                var data = departments.Select(d => ToDto(d, counts.TryGetValue(d.Id, out var c) ? c : 0)).ToList();
                return Ok(data, "Departments found.");
            }
            catch (Exception ex)
            {
                return Unexpected<List<DepartmentDto>>(ex, "load departments");
            }
        }

        public async Task<BaseResponse<List<DepartmentBudgetDto>>> GetBudgetsAsync(int? year, int? month, bool includeInactive)
        {
            try
            {
                var (y, m) = Period(year, month);
                var data = await _budgetService.GetAllDepartmentBudgetsAsync(y, m, includeInactive);
                return Ok(data, "Budgets calculated.");
            }
            catch (Exception ex)
            {
                return Unexpected<List<DepartmentBudgetDto>>(ex, "calculate budgets");
            }
        }

        public async Task<BaseResponse<DepartmentBudgetDto>> GetBudgetAsync(Guid id, int? year, int? month)
        {
            try
            {
                var (y, m) = Period(year, month);
                var data = await _budgetService.GetDepartmentBudgetAsync(id, y, m);
                return data == null ? Fail<DepartmentBudgetDto>("Department not found.") : Ok(data, "Budget calculated.");
            }
            catch (Exception ex)
            {
                return Unexpected<DepartmentBudgetDto>(ex, "calculate the budget");
            }
        }

        public async Task<BaseResponse<DepartmentDto>> CreateAsync(CreateDepartmentModel model, Guid userId)
        {
            try
            {
                var error = Validate(model, out var name, out var annual, out var monthly);
                if (error != null)
                {
                    return Fail<DepartmentDto>(error);
                }

                var normalized = NameNormalizer.Normalize(name);
                var existing = await _departmentRepository.GetByNormalizedNameAsync(normalized);
                if (existing != null)
                {
                    return Fail<DepartmentDto>(existing.IsActive
                        ? $"A department named \"{existing.Name}\" already exists."
                        : $"A department named \"{existing.Name}\" already exists but is inactive. Reactivate it instead of creating a new one.");
                }

                var department = new Department
                {
                    Name = name,
                    NormalizedName = normalized,
                    Description = Clean(model.Description),
                    AnnualBudget = annual,
                    MonthlyBudget = monthly,
                    IsActive = true
                };

                await _departmentRepository.CreateAsync(department);
                await _auditLogRepository.CreateAsync(new AuditLog
                {
                    EntityName = nameof(Department),
                    EntityId = department.Id,
                    Action = "Create",
                    UserId = userId,
                    Details = $"Created {name}: annual budget {annual:0.00}{(monthly.HasValue ? $", monthly {monthly:0.00}" : string.Empty)}"
                });

                var saveError = await TrySaveAsync();
                return saveError != null ? Fail<DepartmentDto>(saveError) : Ok(ToDto(department, 0), "Department created.");
            }
            catch (Exception ex)
            {
                return Unexpected<DepartmentDto>(ex, "create the department");
            }
        }

        public async Task<BaseResponse<DepartmentDto>> UpdateAsync(UpdateDepartmentModel model, Guid userId)
        {
            try
            {
                var department = await _departmentRepository.GetByIdAsync(model.Id);
                if (department == null)
                {
                    return Fail<DepartmentDto>("Department not found.");
                }

                var error = Validate(model, out var name, out var annual, out var monthly);
                if (error != null)
                {
                    return Fail<DepartmentDto>(error);
                }

                var normalized = NameNormalizer.Normalize(name);
                var clash = await _departmentRepository.GetByNormalizedNameAsync(normalized);
                if (clash != null && clash.Id != department.Id)
                {
                    return Fail<DepartmentDto>($"Another department is already called \"{clash.Name}\".");
                }

                var changes = new List<string>();
                if (department.Name != name) changes.Add($"name \"{department.Name}\" -> \"{name}\"");
                if (department.AnnualBudget != annual) changes.Add($"annual budget {department.AnnualBudget:0.00} -> {annual:0.00}");
                if (department.MonthlyBudget != monthly) changes.Add($"monthly budget {department.MonthlyBudget?.ToString("0.00") ?? "none"} -> {monthly?.ToString("0.00") ?? "none"}");
                if (department.IsActive != model.IsActive) changes.Add(model.IsActive ? "reactivated" : "deactivated");

                department.Name = name;
                department.NormalizedName = normalized;
                department.Description = Clean(model.Description);
                department.AnnualBudget = annual;
                department.MonthlyBudget = monthly;
                department.IsActive = model.IsActive;
                department.UpdatedDate = DateTime.UtcNow;

                await _auditLogRepository.CreateAsync(new AuditLog
                {
                    EntityName = nameof(Department),
                    EntityId = department.Id,
                    Action = model.IsActive ? "Update" : "Deactivate",
                    UserId = userId,
                    Details = changes.Count == 0 ? "No changes." : string.Join("; ", changes)
                });

                var saveError = await TrySaveAsync();
                if (saveError != null)
                {
                    return Fail<DepartmentDto>(saveError);
                }

                var counts = await _departmentRepository.GetRequestCountsAsync();
                return Ok(ToDto(department, counts.TryGetValue(department.Id, out var c) ? c : 0), "Department updated.");
            }
            catch (Exception ex)
            {
                return Unexpected<DepartmentDto>(ex, "update the department");
            }
        }

        public async Task<BaseResponse<List<LegacyDepartmentNameDto>>> GetLegacyNamesAsync()
        {
            try
            {
                var rows = await _reimbursementRepository.GetReportRowsAsync(r =>
                    r.DepartmentId == null && r.Classification == ReimbursementClassification.Department);

                // Drafts are excluded from report rows, so add unlinked drafts too.
                var drafts = await _reimbursementRepository.GetAllAsync(r =>
                    !r.IsDeleted && r.Status == ReimbursementStatus.Draft
                    && r.DepartmentId == null && r.Classification == ReimbursementClassification.Department);

                var all = rows.Select(r => (Name: r.DepartmentText, Amount: r.TotalAmount))
                    .Concat(drafts.Select(d => (Name: d.Department, Amount: d.TotalAmount)))
                    .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                    .ToList();

                var departments = await _departmentRepository.GetAllOrderedAsync(activeOnly: false);
                var byNormalized = departments.ToDictionary(d => d.NormalizedName);

                var data = all
                    .GroupBy(x => NameNormalizer.Normalize(x.Name))
                    .Select(g =>
                    {
                        var label = g.GroupBy(x => NameNormalizer.Clean(x.Name)).OrderByDescending(v => v.Count()).First().Key;
                        byNormalized.TryGetValue(g.Key, out var suggestion);
                        return new LegacyDepartmentNameDto
                        {
                            LegacyName = label,
                            RequestCount = g.Count(),
                            TotalAmount = g.Sum(x => x.Amount),
                            SuggestedDepartmentId = suggestion?.Id,
                            SuggestedDepartmentName = suggestion?.Name
                        };
                    })
                    .OrderByDescending(x => x.RequestCount)
                    .ThenBy(x => x.LegacyName)
                    .ToList();

                return Ok(data, data.Count == 0 ? "Every request is linked to a department." : "Unlinked department names found.");
            }
            catch (Exception ex)
            {
                return Unexpected<List<LegacyDepartmentNameDto>>(ex, "load unlinked department names");
            }
        }

        public async Task<BaseResponse<int>> LinkLegacyAsync(LinkLegacyDepartmentModel model, Guid userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.LegacyName))
                {
                    return Fail<int>("Choose the typed department name to link.");
                }

                Department? department = null;
                if (!model.PersonalUse)
                {
                    if (model.DepartmentId == null)
                    {
                        return Fail<int>("Choose a department, or mark these requests as Personal Use.");
                    }

                    department = await _departmentRepository.GetByIdAsync(model.DepartmentId.Value);
                    if (department == null)
                    {
                        return Fail<int>("Department not found.");
                    }
                }

                var key = NameNormalizer.Normalize(model.LegacyName);
                var requests = (await _reimbursementRepository.GetUnlinkedDepartmentRequestsAsync())
                    .Where(r => NameNormalizer.Normalize(r.Department) == key)
                    .ToList();

                if (requests.Count == 0)
                {
                    return Fail<int>("No unlinked requests use that department name any more.");
                }

                var names = await _userRepository.GetDisplayNamesAsync([userId]);
                var actorName = names.TryGetValue(userId, out var n) ? n : null;
                var target = department?.Name ?? ReimbursementService.PersonalUseLabel;

                foreach (var request in requests)
                {
                    if (department != null)
                    {
                        request.DepartmentId = department.Id;
                    }
                    else
                    {
                        request.Classification = ReimbursementClassification.PersonalUse;
                    }

                    // The original typed text stays in the Department column for history.
                    _reimbursementRepository.TrackAsAdded(new ReimbursementHistory
                    {
                        ReimbursementRequestId = request.Id,
                        ActorUserId = userId,
                        ActorName = actorName,
                        Action = "DepartmentLinked",
                        Notes = $"Typed department \"{request.Department}\" linked to {target}.",
                        Timestamp = DateTime.UtcNow
                    });
                }

                await _auditLogRepository.CreateAsync(new AuditLog
                {
                    EntityName = nameof(ReimbursementRequest),
                    EntityId = department?.Id ?? Guid.Empty,
                    Action = "LinkLegacyDepartment",
                    UserId = userId,
                    Details = $"Linked {requests.Count} request(s) typed as \"{model.LegacyName}\" to {target}."
                });

                var saveError = await TrySaveAsync();
                return saveError != null
                    ? Fail<int>(saveError)
                    : Ok(requests.Count, $"Linked {requests.Count} request(s) to {target}.");
            }
            catch (Exception ex)
            {
                return Unexpected<int>(ex, "link the department name");
            }
        }

        private static string? Validate(CreateDepartmentModel model, out string name, out decimal annual, out decimal? monthly)
        {
            name = NameNormalizer.Clean(model.Name);
            annual = 0;
            monthly = null;

            if (name.Length == 0)
            {
                return "Department name is required.";
            }

            if (name.Length > MaxNameLength)
            {
                return $"Department name must be {MaxNameLength} characters or fewer.";
            }

            if (name.Equals(ReimbursementService.PersonalUseLabel, StringComparison.OrdinalIgnoreCase))
            {
                return "\"Personal Use\" is a built-in option on the reimbursement form, not a department.";
            }

            if (model.AnnualBudget == null)
            {
                return "Annual budget is required. Enter 0 if the department has no budget.";
            }

            if (model.AnnualBudget < 0 || model.AnnualBudget > MaxBudget)
            {
                return "Annual budget must be zero or more.";
            }

            if (model.MonthlyBudget < 0 || model.MonthlyBudget > MaxBudget)
            {
                return "Monthly budget must be zero or more, or left empty.";
            }

            annual = Math.Round(model.AnnualBudget.Value, 2, MidpointRounding.AwayFromZero);
            monthly = model.MonthlyBudget.HasValue
                ? Math.Round(model.MonthlyBudget.Value, 2, MidpointRounding.AwayFromZero)
                : null;
            return null;
        }

        private async Task<string?> TrySaveAsync()
        {
            try
            {
                await _unitOfWork.SaveChangesAsync();
                return null;
            }
            catch (DbUpdateConcurrencyException)
            {
                return "These records were changed by someone else at the same moment. Refresh and try again.";
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("IX_Departments_NormalizedName") == true)
            {
                return "A department with that name already exists.";
            }
        }

        private static (int Year, int Month) Period(int? year, int? month)
        {
            var now = DateTime.UtcNow;
            var y = year is >= 2000 and <= 2100 ? year.Value : now.Year;
            var m = month is >= 1 and <= 12 ? month.Value : (y == now.Year ? now.Month : 12);
            return (y, m);
        }

        private static string? Clean(string? value)
        {
            var cleaned = value?.Trim();
            return string.IsNullOrEmpty(cleaned) ? null : cleaned;
        }

        private static DepartmentDto ToDto(Department d, int requestCount) => new()
        {
            Id = d.Id,
            Name = d.Name,
            Description = d.Description,
            AnnualBudget = d.AnnualBudget,
            MonthlyBudget = d.MonthlyBudget,
            IsActive = d.IsActive,
            CreatedDate = d.CreatedDate,
            UpdatedDate = d.UpdatedDate,
            RequestCount = requestCount
        };

        private static BaseResponse<T> Ok<T>(T data, string message) => new() { Message = message, Status = true, Data = data };
        private static BaseResponse<T> Fail<T>(string message) => new() { Message = message, Status = false, Data = default };

        private BaseResponse<T> Unexpected<T>(Exception ex, string action)
        {
            _logger.LogError(ex, "Failed to {Action}.", action);
            return Fail<T>($"Something went wrong while trying to {action}. Please try again.");
        }
    }
}
