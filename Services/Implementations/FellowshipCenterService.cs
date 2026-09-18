using ChurchPortal.Core.Dtos;
using ChurchPortal.Core.Dtos.Request;
using ChurchPortal.Core.Entities;
using ChurchPortal.Repositories.Interfaces;
using ChurchPortal.Services.Interfaces;

namespace ChurchPortal.Services.Implementations
{
    public class FellowshipCenterService : IFellowshipCenterService
    {
        private readonly IFellowshipCenterRepository _fellowshipCenterRepository;
        private readonly IFellowshipAttendanceRepository _fellowshipAttendanceRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IUnitOfWork _unitOfWork;

        public FellowshipCenterService(
            IFellowshipCenterRepository fellowshipCenterRepository,
            IFellowshipAttendanceRepository fellowshipAttendanceRepository,
            IAuditLogRepository auditLogRepository,
            IUnitOfWork unitOfWork)
        {
            _fellowshipCenterRepository = fellowshipCenterRepository;
            _fellowshipAttendanceRepository = fellowshipAttendanceRepository;
            _auditLogRepository = auditLogRepository;
            _unitOfWork = unitOfWork;
        }

        private static FellowshipCenterDto ToDto(FellowshipCenter c) => new FellowshipCenterDto
        {
            Id = c.Id,
            CenterName = c.CenterName,
            Zone = c.Zone,
            LeaderName = c.LeaderName,
            Location = c.Location
        };

        public async Task<BaseResponse<FellowshipCenterDto>> CreateAsync(CreateFellowshipCenterRequestModel model, Guid userId)
        {
            try
            {
                if (Validator.CheckNull(model) || Validator.CheckString(model.CenterName))
                {
                    return new BaseResponse<FellowshipCenterDto> { Message = "Center name is required.", Status = false, Data = null };
                }

                var duplicate = await _fellowshipCenterRepository.CheckAsync(a => a.CenterName == model.CenterName && !a.IsDeleted);
                if (Validator.CheckDuplicate(duplicate))
                {
                    return new BaseResponse<FellowshipCenterDto> { Message = "A fellowship center with this name already exists.", Status = false, Data = null };
                }

                var center = new FellowshipCenter
                {
                    CenterName = model.CenterName,
                    Zone = model.Zone,
                    LeaderName = model.LeaderName,
                    Location = model.Location
                };

                await _fellowshipCenterRepository.CreateAsync(center);
                await _auditLogRepository.CreateAsync(new AuditLog
                {
                    EntityName = nameof(FellowshipCenter),
                    EntityId = center.Id,
                    Action = "Create",
                    UserId = userId
                });
                await _unitOfWork.SaveChangesAsync();

                return new BaseResponse<FellowshipCenterDto> { Message = "Fellowship center created successfully.", Status = true, Data = ToDto(center) };
            }
            catch (Exception ex)
            {
                return new BaseResponse<FellowshipCenterDto> { Message = ex.Message, Status = false, Data = null };
            }
        }

        public async Task<BaseResponse<ICollection<FellowshipCenterDto>>> GetAll()
        {
            try
            {
                var centers = await _fellowshipCenterRepository.GetAllAsync(a => !a.IsDeleted);
                return new BaseResponse<ICollection<FellowshipCenterDto>> { Message = "Fellowship centers found", Status = true, Data = centers.Select(ToDto).ToList() };
            }
            catch (Exception ex)
            {
                return new BaseResponse<ICollection<FellowshipCenterDto>> { Message = ex.Message, Status = false, Data = null };
            }
        }

        public async Task<BaseResponse<FellowshipCenterDto>> GetAsync(Guid id)
        {
            var center = await _fellowshipCenterRepository.GetByIdAsync(id);
            if (center == null)
            {
                return new BaseResponse<FellowshipCenterDto> { Message = "Fellowship center not found.", Status = false, Data = null };
            }
            return new BaseResponse<FellowshipCenterDto> { Message = "Fellowship center found", Status = true, Data = ToDto(center) };
        }

        public async Task<BaseResponse<FellowshipCenterDto>> UpdateAsync(UpdateFellowshipCenterModel model, Guid userId)
        {
            try
            {
                var center = await _fellowshipCenterRepository.GetByIdAsync(model.Id);
                if (center == null)
                {
                    return new BaseResponse<FellowshipCenterDto> { Message = "Fellowship center not found.", Status = false, Data = null };
                }

                center.CenterName = model.CenterName;
                center.Zone = model.Zone;
                center.LeaderName = model.LeaderName;
                center.Location = model.Location;

                await _fellowshipCenterRepository.Update(center);
                await _auditLogRepository.CreateAsync(new AuditLog
                {
                    EntityName = nameof(FellowshipCenter),
                    EntityId = center.Id,
                    Action = "Update",
                    UserId = userId
                });
                await _unitOfWork.SaveChangesAsync();

                return new BaseResponse<FellowshipCenterDto> { Message = "Fellowship center updated successfully.", Status = true, Data = ToDto(center) };
            }
            catch (Exception ex)
            {
                return new BaseResponse<FellowshipCenterDto> { Message = ex.Message, Status = false, Data = null };
            }
        }

        public async Task<BaseResponse<bool>> SoftDeleteAsync(Guid id, Guid userId)
        {
            try
            {
                var center = await _fellowshipCenterRepository.GetByIdAsync(id);
                if (center == null)
                {
                    return new BaseResponse<bool> { Message = "Fellowship center not found.", Status = false, Data = false };
                }

                var hasAttendance = await _fellowshipAttendanceRepository.CheckAsync(a => a.FellowshipCenterId == id && !a.IsDeleted);
                if (hasAttendance)
                {
                    return new BaseResponse<bool> { Message = "Cannot delete this fellowship center because attendance records depend on it.", Status = false, Data = false };
                }

                await _fellowshipCenterRepository.SoftDeleteAsync(center);
                await _auditLogRepository.CreateAsync(new AuditLog
                {
                    EntityName = nameof(FellowshipCenter),
                    EntityId = center.Id,
                    Action = "Delete",
                    UserId = userId
                });
                await _unitOfWork.SaveChangesAsync();

                return new BaseResponse<bool> { Message = "Fellowship center deleted successfully.", Status = true, Data = true };
            }
            catch (Exception ex)
            {
                return new BaseResponse<bool> { Message = ex.Message, Status = false, Data = false };
            }
        }
    }
}
