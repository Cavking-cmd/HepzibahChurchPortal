using ChurchPortal.Core.Dtos;
using ChurchPortal.Core.Dtos.Request;
using ChurchPortal.Core.Entities;
using ChurchPortal.Repositories.Interfaces;
using ChurchPortal.Services.Interfaces;

namespace ChurchPortal.Services.Implementations
{
    public class FellowshipAttendanceService : IFellowshipAttendanceService
    {
        private readonly IFellowshipAttendanceRepository _fellowshipAttendanceRepository;
        private readonly IFellowshipCenterRepository _fellowshipCenterRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IUnitOfWork _unitOfWork;

        public FellowshipAttendanceService(
            IFellowshipAttendanceRepository fellowshipAttendanceRepository,
            IFellowshipCenterRepository fellowshipCenterRepository,
            IAuditLogRepository auditLogRepository,
            IUnitOfWork unitOfWork)
        {
            _fellowshipAttendanceRepository = fellowshipAttendanceRepository;
            _fellowshipCenterRepository = fellowshipCenterRepository;
            _auditLogRepository = auditLogRepository;
            _unitOfWork = unitOfWork;
        }

        private static FellowshipAttendanceDto ToDto(FellowshipAttendance a) => new FellowshipAttendanceDto
        {
            Id = a.Id,
            FellowshipCenterId = a.FellowshipCenterId,
            Date = a.Date,
            Men = a.Men,
            Women = a.Women,
            Children = a.Children,
            NewConverts = a.NewConverts,
            Total = a.Total,
            IsApproved = a.IsApproved,
            IsLocked = a.IsLocked,
            ApprovedByUserId = a.ApprovedByUserId,
            ApprovedDate = a.ApprovedDate
        };

        public async Task<BaseResponse<FellowshipAttendanceDto>> CreateAsync(CreateFellowshipAttendanceRequestModel model, Guid userId)
        {
            try
            {
                if (Validator.CheckNull(model))
                {
                    return new BaseResponse<FellowshipAttendanceDto> { Message = "Fellowship attendance model cannot be null.", Status = false, Data = null };
                }

                var centerExists = await _fellowshipCenterRepository.CheckAsync(a => a.Id == model.FellowshipCenterId && !a.IsDeleted);
                if (!centerExists)
                {
                    return new BaseResponse<FellowshipAttendanceDto> { Message = "The specified fellowship center does not exist.", Status = false, Data = null };
                }

                var attendance = new FellowshipAttendance
                {
                    FellowshipCenterId = model.FellowshipCenterId,
                    Date = Validator.AsUtc(model.Date),
                    Men = model.Men,
                    Women = model.Women,
                    Children = model.Children,
                    NewConverts = model.NewConverts,
                    Total = model.Men + model.Women + model.Children
                };

                await _fellowshipAttendanceRepository.CreateAsync(attendance);
                await _auditLogRepository.CreateAsync(new AuditLog
                {
                    EntityName = nameof(FellowshipAttendance),
                    EntityId = attendance.Id,
                    Action = "Create",
                    UserId = userId
                });
                await _unitOfWork.SaveChangesAsync();

                return new BaseResponse<FellowshipAttendanceDto> { Message = "Fellowship attendance recorded successfully.", Status = true, Data = ToDto(attendance) };
            }
            catch (Exception ex)
            {
                return new BaseResponse<FellowshipAttendanceDto> { Message = ex.Message, Status = false, Data = null };
            }
        }

        public async Task<BaseResponse<ICollection<FellowshipAttendanceDto>>> GetAll()
        {
            try
            {
                var attendances = await _fellowshipAttendanceRepository.GetAllWithCenterAsync();
                return new BaseResponse<ICollection<FellowshipAttendanceDto>> { Message = "Fellowship attendance records found", Status = true, Data = attendances.Select(ToDto).ToList() };
            }
            catch (Exception ex)
            {
                return new BaseResponse<ICollection<FellowshipAttendanceDto>> { Message = ex.Message, Status = false, Data = null };
            }
        }

        public async Task<BaseResponse<FellowshipAttendanceDto>> GetAsync(Guid id)
        {
            var attendance = await _fellowshipAttendanceRepository.GetByIdAsync(id);
            if (attendance == null)
            {
                return new BaseResponse<FellowshipAttendanceDto> { Message = "Fellowship attendance record not found.", Status = false, Data = null };
            }
            return new BaseResponse<FellowshipAttendanceDto> { Message = "Fellowship attendance record found", Status = true, Data = ToDto(attendance) };
        }

        public async Task<BaseResponse<FellowshipAttendanceDto>> UpdateAsync(UpdateFellowshipAttendanceModel model, Guid userId)
        {
            try
            {
                var attendance = await _fellowshipAttendanceRepository.GetByIdAsync(model.Id);
                if (attendance == null)
                {
                    return new BaseResponse<FellowshipAttendanceDto> { Message = "Fellowship attendance record not found.", Status = false, Data = null };
                }

                if (attendance.IsLocked)
                {
                    return new BaseResponse<FellowshipAttendanceDto> { Message = "This fellowship attendance record is locked and cannot be edited.", Status = false, Data = null };
                }

                attendance.FellowshipCenterId = model.FellowshipCenterId;
                attendance.Date = Validator.AsUtc(model.Date);
                attendance.Men = model.Men;
                attendance.Women = model.Women;
                attendance.Children = model.Children;
                attendance.NewConverts = model.NewConverts;
                attendance.Total = model.Men + model.Women + model.Children;

                await _fellowshipAttendanceRepository.Update(attendance);
                await _auditLogRepository.CreateAsync(new AuditLog
                {
                    EntityName = nameof(FellowshipAttendance),
                    EntityId = attendance.Id,
                    Action = "Update",
                    UserId = userId
                });
                await _unitOfWork.SaveChangesAsync();

                return new BaseResponse<FellowshipAttendanceDto> { Message = "Fellowship attendance record updated successfully.", Status = true, Data = ToDto(attendance) };
            }
            catch (Exception ex)
            {
                return new BaseResponse<FellowshipAttendanceDto> { Message = ex.Message, Status = false, Data = null };
            }
        }

        public async Task<BaseResponse<bool>> SoftDeleteAsync(Guid id, Guid userId)
        {
            try
            {
                var attendance = await _fellowshipAttendanceRepository.GetByIdAsync(id);
                if (attendance == null)
                {
                    return new BaseResponse<bool> { Message = "Fellowship attendance record not found.", Status = false, Data = false };
                }

                if (attendance.IsLocked)
                {
                    return new BaseResponse<bool> { Message = "This fellowship attendance record is locked and cannot be deleted.", Status = false, Data = false };
                }

                await _fellowshipAttendanceRepository.SoftDeleteAsync(attendance);
                await _auditLogRepository.CreateAsync(new AuditLog
                {
                    EntityName = nameof(FellowshipAttendance),
                    EntityId = attendance.Id,
                    Action = "Delete",
                    UserId = userId
                });
                await _unitOfWork.SaveChangesAsync();

                return new BaseResponse<bool> { Message = "Fellowship attendance record deleted successfully.", Status = true, Data = true };
            }
            catch (Exception ex)
            {
                return new BaseResponse<bool> { Message = ex.Message, Status = false, Data = false };
            }
        }

        public async Task<BaseResponse<FellowshipAttendanceDto>> ApproveAsync(Guid id, Guid approverUserId)
        {
            try
            {
                var attendance = await _fellowshipAttendanceRepository.GetByIdAsync(id);
                if (attendance == null)
                {
                    return new BaseResponse<FellowshipAttendanceDto> { Message = "Fellowship attendance record not found.", Status = false, Data = null };
                }

                attendance.IsApproved = true;
                attendance.ApprovedByUserId = approverUserId;
                attendance.ApprovedDate = DateTime.UtcNow;

                await _fellowshipAttendanceRepository.Update(attendance);
                await _auditLogRepository.CreateAsync(new AuditLog
                {
                    EntityName = nameof(FellowshipAttendance),
                    EntityId = attendance.Id,
                    Action = "Approve",
                    UserId = approverUserId
                });
                await _unitOfWork.SaveChangesAsync();

                return new BaseResponse<FellowshipAttendanceDto> { Message = "Fellowship attendance record approved.", Status = true, Data = ToDto(attendance) };
            }
            catch (Exception ex)
            {
                return new BaseResponse<FellowshipAttendanceDto> { Message = ex.Message, Status = false, Data = null };
            }
        }

        public async Task<BaseResponse<FellowshipAttendanceDto>> LockAsync(Guid id, Guid userId)
        {
            try
            {
                var attendance = await _fellowshipAttendanceRepository.GetByIdAsync(id);
                if (attendance == null)
                {
                    return new BaseResponse<FellowshipAttendanceDto> { Message = "Fellowship attendance record not found.", Status = false, Data = null };
                }

                if (!attendance.IsApproved)
                {
                    return new BaseResponse<FellowshipAttendanceDto> { Message = "Only approved fellowship attendance records can be locked.", Status = false, Data = null };
                }

                if (attendance.IsLocked)
                {
                    return new BaseResponse<FellowshipAttendanceDto> { Message = "This fellowship attendance record is already locked.", Status = false, Data = null };
                }

                attendance.IsLocked = true;

                await _fellowshipAttendanceRepository.Update(attendance);
                await _auditLogRepository.CreateAsync(new AuditLog
                {
                    EntityName = nameof(FellowshipAttendance),
                    EntityId = attendance.Id,
                    Action = "Lock",
                    UserId = userId
                });
                await _unitOfWork.SaveChangesAsync();

                return new BaseResponse<FellowshipAttendanceDto> { Message = "Fellowship attendance record locked.", Status = true, Data = ToDto(attendance) };
            }
            catch (Exception ex)
            {
                return new BaseResponse<FellowshipAttendanceDto> { Message = ex.Message, Status = false, Data = null };
            }
        }
    }
}
