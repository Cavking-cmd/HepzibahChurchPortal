using ChurchPortal.Core.Dtos;
using ChurchPortal.Core.Dtos.Request;

namespace ChurchPortal.Services.Interfaces
{
    public interface IReimbursementService
    {
        Task<BaseResponse<ReimbursementRequestDto>> CreateAsync(CreateReimbursementRequestModel model, Guid userId);
        Task<BaseResponse<ReimbursementRequestDto>> UpdateDraftAsync(UpdateReimbursementRequestModel model, Guid userId);
        Task<BaseResponse<ReimbursementRequestDto>> GetAsync(Guid id, Guid userId, IReadOnlyCollection<string> roles);
        Task<BaseResponse<ICollection<ReimbursementRequestDto>>> GetAllAsync(Guid userId, IReadOnlyCollection<string> roles);
        Task<BaseResponse<ReimbursementRequestDto>> SubmitAsync(Guid id, Guid userId);
        Task<BaseResponse<ReimbursementRequestDto>> MinisterApproveAsync(Guid id, ReimbursementDecisionModel model, Guid userId);
        Task<BaseResponse<ReimbursementRequestDto>> RequestInfoAsync(Guid id, RequestReimbursementInfoModel model, Guid userId);
        Task<BaseResponse<ReimbursementRequestDto>> RejectAsync(Guid id, RejectReimbursementModel model, Guid userId);
        Task<BaseResponse<ReimbursementRequestDto>> ApproveAsync(Guid id, ReimbursementDecisionModel model, Guid userId);
        Task<BaseResponse<ReimbursementRequestDto>> RecordPaymentAsync(Guid id, RecordReimbursementPaymentModel model, Guid userId);
        Task<BaseResponse<bool>> DeleteDraftAsync(Guid id, Guid userId);
        Task<BaseResponse<ReceiptFileDto>> GetReceiptAsync(Guid receiptId, Guid userId, IReadOnlyCollection<string> roles);
        Task<BaseResponse<ICollection<NotificationDto>>> GetNotificationsAsync(Guid userId);
        Task<BaseResponse<int>> GetUnreadNotificationCountAsync(Guid userId);
        Task<BaseResponse<bool>> MarkNotificationsReadAsync(Guid userId);
    }
}
