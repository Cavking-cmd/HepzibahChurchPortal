using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using ChurchPortal.Core.Dtos;
using ChurchPortal.Core.Dtos.Request;
using ChurchPortal.Core.Entities;
using ChurchPortal.Repositories.Interfaces;
using ChurchPortal.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace ChurchPortal.Services.Implementations
{
    /// <summary>
    /// Pastor name records used on services. Duplicate names are not blocked outright,
    /// because two different people can share a name; instead the user is warned and must
    /// either pick the existing record or confirm this is someone else.
    /// </summary>
    public class PastorService : IPastorService
    {
        private static readonly Regex PhonePattern = new(@"^[0-9+()\-.\s]{5,40}$", RegexOptions.Compiled);

        private readonly IPastorRepository _pastorRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PastorService> _logger;

        public PastorService(
            IPastorRepository pastorRepository,
            IAuditLogRepository auditLogRepository,
            IUnitOfWork unitOfWork,
            ILogger<PastorService> logger)
        {
            _pastorRepository = pastorRepository;
            _auditLogRepository = auditLogRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<BaseResponse<List<PastorDto>>> GetAllAsync(bool includeInactive, string? search)
        {
            try
            {
                var pastors = await _pastorRepository.GetAllOrderedAsync(activeOnly: !includeInactive);
                var counts = await _pastorRepository.GetServiceCountsAsync();
                var term = NameNormalizer.Normalize(search);
                var data = pastors
                    .Where(p => term.Length == 0 || p.NormalizedName.Contains(term) || NameNormalizer.ComparisonKey(p.Name).Contains(term))
                    .Select(p => ToDto(p, counts))
                    .ToList();
                return Ok(data, "Pastors found.");
            }
            catch (Exception ex)
            {
                return Unexpected<List<PastorDto>>(ex, "load pastors");
            }
        }

        public async Task<BaseResponse<List<PastorDto>>> FindPossibleDuplicatesAsync(string? name, Guid? excludeId)
        {
            try
            {
                var matches = await FindMatchesAsync(name, excludeId);
                return Ok(matches, matches.Count == 0 ? "No similar names." : "Similar names found.");
            }
            catch (Exception ex)
            {
                return Unexpected<List<PastorDto>>(ex, "check for duplicates");
            }
        }

        public async Task<PastorSaveResult> CreateAsync(CreatePastorModel model, Guid userId)
        {
            try
            {
                var error = Validate(model, out var cleaned);
                if (error != null)
                {
                    return Result(Fail<PastorDto>(error));
                }

                if (!model.ConfirmNotDuplicate)
                {
                    var matches = await FindMatchesAsync(cleaned.Name, null);
                    if (matches.Count > 0)
                    {
                        return DuplicateResult(cleaned.Name, matches);
                    }
                }

                var pastor = new Pastor
                {
                    Name = cleaned.Name,
                    NormalizedName = NameNormalizer.Normalize(cleaned.Name),
                    Email = cleaned.Email,
                    PhoneNumber = cleaned.Phone,
                    Description = cleaned.Description,
                    IsActive = true
                };

                await _pastorRepository.CreateAsync(pastor);
                await _auditLogRepository.CreateAsync(new AuditLog
                {
                    EntityName = nameof(Pastor),
                    EntityId = pastor.Id,
                    Action = "Create",
                    UserId = userId,
                    Details = model.ConfirmNotDuplicate ? $"Created {pastor.Name} (confirmed not a duplicate)" : $"Created {pastor.Name}"
                });
                await _unitOfWork.SaveChangesAsync();

                return Result(Ok(ToDto(pastor, null), "Pastor added."));
            }
            catch (Exception ex)
            {
                return Result(Unexpected<PastorDto>(ex, "add the pastor"));
            }
        }

        public async Task<PastorSaveResult> UpdateAsync(UpdatePastorModel model, Guid userId)
        {
            try
            {
                var pastor = await _pastorRepository.GetByIdAsync(model.Id);
                if (pastor == null)
                {
                    return Result(Fail<PastorDto>("Pastor not found."));
                }

                var error = Validate(model, out var cleaned);
                if (error != null)
                {
                    return Result(Fail<PastorDto>(error));
                }

                var renamed = pastor.Name != cleaned.Name;
                if (renamed && !model.ConfirmNotDuplicate)
                {
                    var matches = await FindMatchesAsync(cleaned.Name, pastor.Id);
                    if (matches.Count > 0)
                    {
                        return DuplicateResult(cleaned.Name, matches);
                    }
                }

                var oldName = pastor.Name;
                pastor.Name = cleaned.Name;
                pastor.NormalizedName = NameNormalizer.Normalize(cleaned.Name);
                pastor.Email = cleaned.Email;
                pastor.PhoneNumber = cleaned.Phone;
                pastor.Description = cleaned.Description;
                pastor.IsActive = model.IsActive;
                pastor.UpdatedDate = DateTime.UtcNow;

                if (renamed)
                {
                    // Keep the preacher name on linked services in step with the record.
                    foreach (var service in await _pastorRepository.GetServicesForPastorAsync(pastor.Id))
                    {
                        service.Preacher = pastor.Name;
                    }
                }

                await _auditLogRepository.CreateAsync(new AuditLog
                {
                    EntityName = nameof(Pastor),
                    EntityId = pastor.Id,
                    Action = "Update",
                    UserId = userId,
                    Details = renamed ? $"Renamed \"{oldName}\" to \"{pastor.Name}\"" : (model.IsActive ? "Updated" : "Deactivated")
                });
                await _unitOfWork.SaveChangesAsync();

                var counts = await _pastorRepository.GetServiceCountsAsync();
                return Result(Ok(ToDto(pastor, counts), "Pastor updated."));
            }
            catch (Exception ex)
            {
                return Result(Unexpected<PastorDto>(ex, "update the pastor"));
            }
        }

        public async Task<BaseResponse<List<LegacyPreacherDto>>> GetLegacyPreachersAsync()
        {
            try
            {
                var services = await _pastorRepository.GetUnlinkedServicesWithPreacherAsync();
                var pastors = await _pastorRepository.GetAllOrderedAsync(activeOnly: false);
                var counts = await _pastorRepository.GetServiceCountsAsync();

                var data = services
                    .GroupBy(s => NameNormalizer.Normalize(s.Preacher))
                    .Where(g => g.Key.Length > 0)
                    .Select(g =>
                    {
                        var label = g.GroupBy(s => NameNormalizer.Clean(s.Preacher)).OrderByDescending(v => v.Count()).First().Key;
                        var key = NameNormalizer.ComparisonKey(label);
                        return new LegacyPreacherDto
                        {
                            PreacherName = label,
                            ServiceCount = g.Count(),
                            FirstServiceDate = g.Min(s => s.Date),
                            LastServiceDate = g.Max(s => s.Date),
                            SuggestedPastors = pastors
                                .Where(p => NameNormalizer.ComparisonKey(p.Name) == key)
                                .Select(p => ToDto(p, counts))
                                .ToList()
                        };
                    })
                    .OrderByDescending(x => x.ServiceCount)
                    .ThenBy(x => x.PreacherName)
                    .ToList();

                return Ok(data, data.Count == 0 ? "Every service is linked to a pastor." : "Unlinked preacher names found.");
            }
            catch (Exception ex)
            {
                return Unexpected<List<LegacyPreacherDto>>(ex, "load unlinked preacher names");
            }
        }

        public async Task<BaseResponse<int>> LinkLegacyPreacherAsync(LinkLegacyPreacherModel model, Guid userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.PreacherName))
                {
                    return Fail<int>("Choose the typed preacher name to link.");
                }

                var pastor = await _pastorRepository.GetByIdAsync(model.PastorId);
                if (pastor == null)
                {
                    return Fail<int>("Pastor not found.");
                }

                var key = NameNormalizer.Normalize(model.PreacherName);
                var services = (await _pastorRepository.GetUnlinkedServicesWithPreacherAsync())
                    .Where(s => NameNormalizer.Normalize(s.Preacher) == key)
                    .ToList();

                if (services.Count == 0)
                {
                    return Fail<int>("No unlinked services use that name any more.");
                }

                foreach (var service in services)
                {
                    service.PastorId = pastor.Id;
                    service.Preacher = pastor.Name;
                }

                await _auditLogRepository.CreateAsync(new AuditLog
                {
                    EntityName = nameof(Pastor),
                    EntityId = pastor.Id,
                    Action = "LinkLegacyPreacher",
                    UserId = userId,
                    Details = $"Linked {services.Count} service(s) typed as \"{model.PreacherName.Trim()}\" to {pastor.Name}."
                });
                await _unitOfWork.SaveChangesAsync();

                return Ok(services.Count, $"Linked {services.Count} service(s) to {pastor.Name}.");
            }
            catch (Exception ex)
            {
                return Unexpected<int>(ex, "link the preacher name");
            }
        }

        private async Task<List<PastorDto>> FindMatchesAsync(string? name, Guid? excludeId)
        {
            var key = NameNormalizer.ComparisonKey(name);
            if (key.Length == 0)
            {
                return [];
            }

            var pastors = await _pastorRepository.GetAllOrderedAsync(activeOnly: false);
            var counts = await _pastorRepository.GetServiceCountsAsync();
            return pastors
                .Where(p => p.Id != excludeId && NameNormalizer.ComparisonKey(p.Name) == key)
                .Select(p => ToDto(p, counts))
                .ToList();
        }

        private sealed record CleanedPastor(string Name, string? Email, string? Phone, string? Description);

        private static string? Validate(CreatePastorModel model, out CleanedPastor cleaned)
        {
            var name = NameNormalizer.Clean(model.Name);
            var email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim();
            var phone = string.IsNullOrWhiteSpace(model.PhoneNumber) ? null : NameNormalizer.Clean(model.PhoneNumber);
            var description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();
            cleaned = new CleanedPastor(name, email, phone, description);

            if (name.Length == 0) return "Name is required.";
            if (name.Length > 150) return "Name must be 150 characters or fewer.";
            if (email != null && (email.Length > 254 || !new EmailAddressAttribute().IsValid(email)))
                return "Enter a valid email address, or leave it empty.";
            if (phone != null && !PhonePattern.IsMatch(phone))
                return "Enter a valid phone number (digits, spaces, +, -, brackets), or leave it empty.";
            if (description != null && description.Length > 500) return "Description must be 500 characters or fewer.";
            return null;
        }

        private static PastorDto ToDto(Pastor p, IReadOnlyDictionary<Guid, int>? counts) => new()
        {
            Id = p.Id,
            Name = p.Name,
            Email = p.Email,
            PhoneNumber = p.PhoneNumber,
            Description = p.Description,
            IsActive = p.IsActive,
            ServiceCount = counts != null && counts.TryGetValue(p.Id, out var c) ? c : 0,
            CreatedDate = p.CreatedDate,
            UpdatedDate = p.UpdatedDate
        };

        private static PastorSaveResult Result(BaseResponse<PastorDto> response) => new() { Response = response };

        private static PastorSaveResult DuplicateResult(string name, List<PastorDto> matches) => new()
        {
            Response = Fail<PastorDto>("A pastor with a similar name already exists."),
            Duplicate = new PastorDuplicateDto
            {
                Message = $"\"{name}\" looks like {(matches.Count == 1 ? "an existing pastor" : $"{matches.Count} existing pastors")}. " +
                          "Use the existing record, or confirm this is a different person.",
                Matches = matches
            }
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
