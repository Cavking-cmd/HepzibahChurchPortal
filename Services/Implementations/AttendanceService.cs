using ChurchPortal.Core.Dtos;
using ChurchPortal.Core.Dtos.Request;
using ChurchPortal.Core.Entities;
using ChurchPortal.Repositories.Interfaces;
using ChurchPortal.Services.Interfaces;

namespace ChurchPortal.Services.Implementations
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly IServiceRepository _serviceRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AttendanceService(
            IAttendanceRepository attendanceRepository,
            IServiceRepository serviceRepository,
            IAuditLogRepository auditLogRepository,
            IUnitOfWork unitOfWork)
        {
            _attendanceRepository = attendanceRepository;
            _serviceRepository = serviceRepository;
            _auditLogRepository = auditLogRepository;
            _unitOfWork = unitOfWork;
        }

        private static AttendanceDto ToDto(Attendance a) => new AttendanceDto
        {
            Id = a.Id,
            ServiceId = a.ServiceId,
            Men = a.Men,
            Women = a.Women,
            Children = a.Children,
            SundaySchool = a.SundaySchool,
            NewConverts = a.NewConverts,
            FirstTimers = a.FirstTimers,
            Total = a.Total,
            IsApproved = a.IsApproved,
            IsLocked = a.IsLocked,
            ApprovedByUserId = a.ApprovedByUserId,
            ApprovedDate = a.ApprovedDate
        };

        public async Task<BaseResponse<AttendanceDto>> CreateAsync(CreateAttendanceRequestModel model, Guid userId)
        {
            try
            {
                if (Validator.CheckNull(model))
                {
                    return new BaseResponse<AttendanceDto> { Message = "Attendance model cannot be null.", Status = false, Data = null };
                }

                var serviceExists = await _serviceRepository.CheckAsync(a => a.Id == model.ServiceId && !a.IsDeleted);
                if (!serviceExists)
                {
                    return new BaseResponse<AttendanceDto> { Message = "The specified service does not exist.", Status = false, Data = null };
                }

                var attendance = new Attendance
                {
                    ServiceId = model.ServiceId,
                    Men = model.Men,
                    Women = model.Women,
                    Children = model.Children,
                    SundaySchool = model.SundaySchool,
                    NewConverts = model.NewConverts,
                    FirstTimers = model.FirstTimers,
                    Total = model.Men + model.Women + model.Children
                };

                await _attendanceRepository.CreateAsync(attendance);
                await _auditLogRepository.CreateAsync(new AuditLog
                {
                    EntityName = nameof(Attendance),
                    EntityId = attendance.Id,
                    Action = "Create",
                    UserId = userId
                });
                await _unitOfWork.SaveChangesAsync();

                return new BaseResponse<AttendanceDto> { Message = "Attendance recorded successfully.", Status = true, Data = ToDto(attendance) };
            }
            catch (Exception ex)
            {
                return new BaseResponse<AttendanceDto> { Message = ex.Message, Status = false, Data = null };
            }
        }

        public async Task<BaseResponse<ICollection<AttendanceDto>>> GetAll()
        {
            try
            {
                var attendances = await _attendanceRepository.GetAllWithServiceAsync();
                return new BaseResponse<ICollection<AttendanceDto>> { Message = "Attendance records found", Status = true, Data = attendances.Select(ToDto).ToList() };
            }
            catch (Exception ex)
            {
                return new BaseResponse<ICollection<AttendanceDto>> { Message = ex.Message, Status = false, Data = null };
            }
        }

        public async Task<BaseResponse<AttendanceDto>> GetAsync(Guid id)
        {
            var attendance = await _attendanceRepository.GetByIdAsync(id);
            if (attendance == null)
            {
                return new BaseResponse<AttendanceDto> { Message = "Attendance record not found.", Status = false, Data = null };
            }
            return new BaseResponse<AttendanceDto> { Message = "Attendance record found", Status = true, Data = ToDto(attendance) };
        }

        public async Task<BaseResponse<AttendanceDto>> UpdateAsync(UpdateAttendanceModel model, Guid userId)
        {
            try
            {
                var attendance = await _attendanceRepository.GetByIdAsync(model.Id);
                if (attendance == null)
                {
                    return new BaseResponse<AttendanceDto> { Message = "Attendance record not found.", Status = false, Data = null };
                }

                if (attendance.IsLocked)
                {
                    return new BaseResponse<AttendanceDto> { Message = "This attendance record is locked and cannot be edited.", Status = false, Data = null };
                }

                attendance.ServiceId = model.ServiceId;
                attendance.Men = model.Men;
                attendance.Women = model.Women;
                attendance.Children = model.Children;
                attendance.SundaySchool = model.SundaySchool;
                attendance.NewConverts = model.NewConverts;
                attendance.FirstTimers = model.FirstTimers;
                attendance.Total = model.Men + model.Women + model.Children;

                await _attendanceRepository.Update(attendance);
                await _auditLogRepository.CreateAsync(new AuditLog
                {
                    EntityName = nameof(Attendance),
                    EntityId = attendance.Id,
                    Action = "Update",
                    UserId = userId
                });
                await _unitOfWork.SaveChangesAsync();

                return new BaseResponse<AttendanceDto> { Message = "Attendance record updated successfully.", Status = true, Data = ToDto(attendance) };
            }
            catch (Exception ex)
            {
                return new BaseResponse<AttendanceDto> { Message = ex.Message, Status = false, Data = null };
            }
        }

        public async Task<BaseResponse<bool>> SoftDeleteAsync(Guid id, Guid userId)
        {
            try
            {
                var attendance = await _attendanceRepository.GetByIdAsync(id);
                if (attendance == null)
                {
                    return new BaseResponse<bool> { Message = "Attendance record not found.", Status = false, Data = false };
                }

                if (attendance.IsLocked)
                {
                    return new BaseResponse<bool> { Message = "This attendance record is locked and cannot be deleted.", Status = false, Data = false };
                }

                await _attendanceRepository.SoftDeleteAsync(attendance);
                await _auditLogRepository.CreateAsync(new AuditLog
                {
                    EntityName = nameof(Attendance),
                    EntityId = attendance.Id,
                    Action = "Delete",
                    UserId = userId
                });
                await _unitOfWork.SaveChangesAsync();

                return new BaseResponse<bool> { Message = "Attendance record deleted successfully.", Status = true, Data = true };
            }
            catch (Exception ex)
            {
                return new BaseResponse<bool> { Message = ex.Message, Status = false, Data = false };
            }
        }

        public async Task<BaseResponse<AttendanceDto>> ApproveAsync(Guid id, Guid approverUserId)
        {
            try
            {
                var attendance = await _attendanceRepository.GetByIdAsync(id);
                if (attendance == null)
                {
                    return new BaseResponse<AttendanceDto> { Message = "Attendance record not found.", Status = false, Data = null };
                }

                attendance.IsApproved = true;
                attendance.ApprovedByUserId = approverUserId;
                attendance.ApprovedDate = DateTime.UtcNow;

                await _attendanceRepository.Update(attendance);
                await _auditLogRepository.CreateAsync(new AuditLog
                {
                    EntityName = nameof(Attendance),
                    EntityId = attendance.Id,
                    Action = "Approve",
                    UserId = approverUserId
                });
                await _unitOfWork.SaveChangesAsync();

                return new BaseResponse<AttendanceDto> { Message = "Attendance record approved.", Status = true, Data = ToDto(attendance) };
            }
            catch (Exception ex)
            {
                return new BaseResponse<AttendanceDto> { Message = ex.Message, Status = false, Data = null };
            }
        }

        public async Task<BaseResponse<AttendanceDto>> LockAsync(Guid id, Guid userId)
        {
            try
            {
                var attendance = await _attendanceRepository.GetByIdAsync(id);
                if (attendance == null)
                {
                    return new BaseResponse<AttendanceDto> { Message = "Attendance record not found.", Status = false, Data = null };
                }

                if (!attendance.IsApproved)
                {
                    return new BaseResponse<AttendanceDto> { Message = "Only approved attendance records can be locked.", Status = false, Data = null };
                }

                if (attendance.IsLocked)
                {
                    return new BaseResponse<AttendanceDto> { Message = "This attendance record is already locked.", Status = false, Data = null };
                }

                attendance.IsLocked = true;

                await _attendanceRepository.Update(attendance);
                await _auditLogRepository.CreateAsync(new AuditLog
                {
                    EntityName = nameof(Attendance),
                    EntityId = attendance.Id,
                    Action = "Lock",
                    UserId = userId
                });
                await _unitOfWork.SaveChangesAsync();

                return new BaseResponse<AttendanceDto> { Message = "Attendance record locked.", Status = true, Data = ToDto(attendance) };
            }
            catch (Exception ex)
            {
                return new BaseResponse<AttendanceDto> { Message = ex.Message, Status = false, Data = null };
            }
        }
    }
}
