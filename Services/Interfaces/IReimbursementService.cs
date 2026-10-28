using ChurchPortal.Core.Dtos;
using ChurchPortal.Core.Dtos.Request;

namespace ChurchPortal.Services.Interfaces
{
    public interface IReimbursementService
    {
        // Member
        Task<BaseResponse<ReimbursementRequestDto>> CreateAsync(CreateReimbursementRequestModel model, Guid userId);
        Task<BaseResponse<ReimbursementRequestDto>> UpdateDraftAsync(UpdateReimbursementRequestModel model, Guid userId);
        Task<BaseResponse<ReimbursementRequestDto>> SubmitAsync(Guid id, Guid userId);
        Task<BaseResponse<bool>> DeleteDraftAsync(Guid id, Guid userId);

        // Reading
        Task<BaseResponse<ReimbursementRequestDto>> GetAsync(Guid id, Guid userId, IReadOnlyCollection<string> roles);
        Task<BaseResponse<ICollection<ReimbursementRequestDto>>> GetAllAsync(Guid userId, IReadOnlyCollection<string> roles);
        Task<BaseResponse<ReimbursementBudgetDto>> GetBudgetAsync(Guid id, decimal? amount, Guid userId, IReadOnlyCollection<string> roles);
        Task<BaseResponse<ReceiptFileDto>> GetReceiptAsync(Guid receiptId, Guid userId, IReadOnlyCollection<string> roles);

        // Stage two: Accounts Officer (optional; the Accounts Admin can approve without it)
        Task<BaseResponse<ReimbursementRequestDto>> ReviewAsync(Guid id, ReviewReimbursementModel model, Guid userId, IReadOnlyCollection<string> roles);

        // Officer or Admin at Under Review; Admin at Pending Approval
        Task<BaseResponse<ReimbursementRequestDto>> ReturnForCorrectionAsync(Guid id, RequestReimbursementInfoModel model, Guid userId, IReadOnlyCollection<string> roles);
        Task<BaseResponse<ReimbursementRequestDto>> RejectAsync(Guid id, RejectReimbursementModel model, Guid userId, IReadOnlyCollection<string> roles);

        // Stage three: Accounts Admin
        Task<BaseResponse<ReimbursementRequestDto>> ApproveAsync(Guid id, ReimbursementDecisionModel model, Guid userId, IReadOnlyCollection<string> roles);
        Task<BaseResponse<ReimbursementRequestDto>> RecordPaymentAsync(Guid id, RecordReimbursementPaymentModel model, Guid userId, IReadOnlyCollection<string> roles);

        // Inventory Officer: physical item checks
        Task<BaseResponse<ICollection<InventoryVerificationItemDto>>> GetVerificationQueueAsync(bool pendingOnly, Guid userId, IReadOnlyCollection<string> roles);
        Task<BaseResponse<InventoryVerificationItemDto>> VerifyItemAsync(Guid itemId, VerifyReimbursementItemModel model, Guid userId, IReadOnlyCollection<string> roles);
        Task<BaseResponse<InventoryVerificationItemDto>> RejectItemAsync(Guid itemId, RejectReimbursementItemModel model, Guid userId, IReadOnlyCollection<string> roles);

        // Notifications
        Task<BaseResponse<ICollection<NotificationDto>>> GetNotificationsAsync(Guid userId);
        Task<BaseResponse<int>> GetUnreadNotificationCountAsync(Guid userId);
        Task<BaseResponse<bool>> MarkNotificationsReadAsync(Guid userId);
    }
}
