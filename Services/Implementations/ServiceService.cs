using ChurchPortal.Core.Dtos;
using ChurchPortal.Core.Dtos.Request;
using ChurchPortal.Core.Entities;
using ChurchPortal.Repositories.Interfaces;
using ChurchPortal.Services.Interfaces;

namespace ChurchPortal.Services.Implementations
{
    public class ServiceService : IServiceService
    {
        private readonly IServiceRepository _serviceRepository;
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IPastorRepository _pastorRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ServiceService(
            IServiceRepository serviceRepository,
            IAttendanceRepository attendanceRepository,
            IAuditLogRepository auditLogRepository,
            IPastorRepository pastorRepository,
            IUnitOfWork unitOfWork)
        {
            _serviceRepository = serviceRepository;
            _attendanceRepository = attendanceRepository;
            _auditLogRepository = auditLogRepository;
            _pastorRepository = pastorRepository;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Works out the pastor and preacher name to store. A selected pastor must exist and be
        /// active (an inactive pastor already on the service may be kept when editing). Without a
        /// selection, an older service may keep its existing typed name unchanged; new free text
        /// is never saved.
        /// </summary>
        private async Task<(Guid? PastorId, string? Preacher, string? Error)> ResolvePreacherAsync(
            Guid? pastorId, string? typedPreacher, Service? existing)
        {
            if (pastorId.HasValue && pastorId.Value != Guid.Empty)
            {
                var pastor = await _pastorRepository.GetByIdAsync(pastorId.Value);
                if (pastor == null)
                {
                    return (null, null, "The selected pastor does not exist.");
                }

                if (!pastor.IsActive && existing?.PastorId != pastor.Id)
                {
                    return (null, null, $"{pastor.Name} is inactive. Reactivate the pastor or choose another.");
                }

                return (pastor.Id, pastor.Name, null);
            }

            if (existing != null && existing.PastorId == null && !string.IsNullOrWhiteSpace(existing.Preacher)
                && NameNormalizer.Normalize(typedPreacher) == NameNormalizer.Normalize(existing.Preacher))
            {
                return (null, existing.Preacher, null);
            }

            return (null, null, null);
        }

        private static ServiceDto ToDto(Service s) => new ServiceDto
        {
            Id = s.Id,
            Date = s.Date,
            Day = s.Day,
            ServiceType = s.ServiceType,
            Theme = s.Theme,
            ScriptureText = s.ScriptureText,
            PastorId = s.PastorId,
            Preacher = s.Preacher,
            PreacherNeedsLink = s.PastorId == null && !string.IsNullOrWhiteSpace(s.Preacher)
        };

        public async Task<BaseResponse<ServiceDto>> CreateAsync(CreateServiceRequestModel model, Guid userId)
        {
            try
            {
                if (Validator.CheckNull(model) || Validator.CheckString(model.Day))
                {
                    return new BaseResponse<ServiceDto> { Message = "Day is required.", Status = false, Data = null };
                }

                var utcDate = Validator.AsUtc(model.Date);
                var duplicate = await _serviceRepository.CheckAsync(a => a.Date.Date == utcDate.Date && a.ServiceType == model.ServiceType && !a.IsDeleted);
                if (Validator.CheckDuplicate(duplicate))
                {
                    return new BaseResponse<ServiceDto> { Message = "A service with this date and service type already exists.", Status = false, Data = null };
                }

                var preacher = await ResolvePreacherAsync(model.PastorId, model.Preacher, null);
                if (preacher.Error != null)
                {
                    return new BaseResponse<ServiceDto> { Message = preacher.Error, Status = false, Data = null };
                }

                var service = new Service
                {
                    Date = utcDate,
                    Day = model.Day,
                    ServiceType = model.ServiceType,
                    Theme = model.Theme,
                    ScriptureText = model.ScriptureText,
                    PastorId = preacher.PastorId,
                    Preacher = preacher.Preacher
                };

                await _serviceRepository.CreateAsync(service);
                await _auditLogRepository.CreateAsync(new AuditLog
                {
                    EntityName = nameof(Service),
                    EntityId = service.Id,
                    Action = "Create",
                    UserId = userId,
                    Details = $"Created service on {service.Date:d} ({service.ServiceType})"
                });
                await _unitOfWork.SaveChangesAsync();

                return new BaseResponse<ServiceDto> { Message = "Service created successfully.", Status = true, Data = ToDto(service) };
            }
            catch (Exception ex)
            {
                return new BaseResponse<ServiceDto> { Message = ex.Message, Status = false, Data = null };
            }
        }

        public async Task<BaseResponse<ICollection<ServiceDto>>> GetAll()
        {
            try
            {
                var services = await _serviceRepository.GetAllWithAttendancesAsync();
                return new BaseResponse<ICollection<ServiceDto>> { Message = "Services found", Status = true, Data = services.Select(ToDto).ToList() };
            }
            catch (Exception ex)
            {
                return new BaseResponse<ICollection<ServiceDto>> { Message = ex.Message, Status = false, Data = null };
            }
        }

        public async Task<BaseResponse<ServiceDto>> GetAsync(Guid id)
        {
            var service = await _serviceRepository.GetByIdAsync(id);
            if (service == null)
            {
                return new BaseResponse<ServiceDto> { Message = "Service not found.", Status = false, Data = null };
            }
            return new BaseResponse<ServiceDto> { Message = "Service found", Status = true, Data = ToDto(service) };
        }

        public async Task<BaseResponse<ServiceDto>> UpdateAsync(UpdateServiceModel model, Guid userId)
        {
            try
            {
                var service = await _serviceRepository.GetByIdAsync(model.Id);
                if (service == null)
                {
                    return new BaseResponse<ServiceDto> { Message = "Service not found.", Status = false, Data = null };
                }

                var utcDate = Validator.AsUtc(model.Date);
                var duplicate = await _serviceRepository.CheckAsync(a => a.Id != model.Id && a.Date.Date == utcDate.Date && a.ServiceType == model.ServiceType && !a.IsDeleted);
                if (Validator.CheckDuplicate(duplicate))
                {
                    return new BaseResponse<ServiceDto> { Message = "A service with this date and service type already exists.", Status = false, Data = null };
                }

                var preacher = await ResolvePreacherAsync(model.PastorId, model.Preacher, service);
                if (preacher.Error != null)
                {
                    return new BaseResponse<ServiceDto> { Message = preacher.Error, Status = false, Data = null };
                }

                service.Date = utcDate;
                service.Day = model.Day;
                service.ServiceType = model.ServiceType;
                service.Theme = model.Theme;
                service.ScriptureText = model.ScriptureText;
                service.PastorId = preacher.PastorId;
                service.Preacher = preacher.Preacher;

                await _serviceRepository.Update(service);
                await _auditLogRepository.CreateAsync(new AuditLog
                {
                    EntityName = nameof(Service),
                    EntityId = service.Id,
                    Action = "Update",
                    UserId = userId
                });
                await _unitOfWork.SaveChangesAsync();

                return new BaseResponse<ServiceDto> { Message = "Service updated successfully.", Status = true, Data = ToDto(service) };
            }
            catch (Exception ex)
            {
                return new BaseResponse<ServiceDto> { Message = ex.Message, Status = false, Data = null };
            }
        }

        public async Task<BaseResponse<bool>> SoftDeleteAsync(Guid id, Guid userId)
        {
            try
            {
                var service = await _serviceRepository.GetByIdAsync(id);
                if (service == null)
                {
                    return new BaseResponse<bool> { Message = "Service not found.", Status = false, Data = false };
                }

                var hasAttendance = await _attendanceRepository.CheckAsync(a => a.ServiceId == id && !a.IsDeleted);
                if (hasAttendance)
                {
                    return new BaseResponse<bool> { Message = "Cannot delete this service because attendance records depend on it.", Status = false, Data = false };
                }

                await _serviceRepository.SoftDeleteAsync(service);
                await _auditLogRepository.CreateAsync(new AuditLog
                {
                    EntityName = nameof(Service),
                    EntityId = service.Id,
                    Action = "Delete",
                    UserId = userId
                });
                await _unitOfWork.SaveChangesAsync();

                return new BaseResponse<bool> { Message = "Service deleted successfully.", Status = true, Data = true };
            }
            catch (Exception ex)
            {
                return new BaseResponse<bool> { Message = ex.Message, Status = false, Data = false };
            }
        }
    }
}
