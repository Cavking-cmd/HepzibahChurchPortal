using ChurchPortal.Core.Dtos;
using ChurchPortal.Core.Dtos.Request;
using ChurchPortal.Core.Entities;
using ChurchPortal.Repositories.Interfaces;
using ChurchPortal.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ChurchPortal.Services.Implementations
{
    /// <summary>
    /// Reimbursement workflow. Every rule is enforced here, on the server, using the
    /// signed-in user's id and roles from the JWT. Nothing about status, role or
    /// payment sent by the browser is trusted.
    ///
    ///   Draft / Returned for correction --member submits--> Under Review
    ///   Under Review     --Accounts Officer forwards--> Pending Approval (optional help step)
    ///   Under Review     --Accounts Admin approves-----> Approved   (directly, no forward needed)
    ///   Under Review     --Officer or Admin returns----> Returned for correction
    ///   Under Review     --Officer or Admin rejects----> Rejected
    ///   Pending Approval --Accounts Admin approves----> Approved   (not paid)
    ///   Pending Approval --Accounts Admin returns-----> Returned for correction
    ///   Pending Approval --Accounts Admin rejects-----> Rejected
    ///   Approved         --Accounts Admin confirms payment--> Paid
    ///   Rejected and Paid are final.
    ///
    /// Physical line items are checked by the Inventory Officer after submission. While
    /// any are waiting or were rejected, the Accounts Officer cannot forward the request;
    /// an Accounts Admin can still approve it with a written override reason.
    ///
    /// Nobody can act on their own request, and the person who reviewed a request
    /// cannot also give its final approval.
    /// </summary>
    public class ReimbursementService : IReimbursementService
    {
        public const string ChurchMemberRole = "ChurchMember";
        public const string AccountsOfficerRole = "AccountsOfficer";
        public const string AccountsAdminRole = "AccountsAdmin";
        public const string InventoryOfficerRole = "InventoryOfficer";
        public const string PersonalUseLabel = "Personal Use";

        private const int MaxItems = 10;
        private const long MaxReceiptBytes = 5 * 1024 * 1024;
        private const string ConcurrencyMessage =
            "This request was changed by someone else at the same moment. Refresh it and try again.";

        private static readonly HashSet<string> AllowedReceiptTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "application/pdf",
            "image/jpeg",
            "image/png",
            "image/webp"
        };

        private readonly IReimbursementRepository _reimbursementRepository;
        private readonly IUserRepository _userRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IInventoryItemRepository _inventoryItemRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IBudgetService _budgetService;
        private readonly IEmailSender _emailSender;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ReimbursementService> _logger;

        // Emails are only sent after the database change has been saved.
        private readonly List<(string To, string Title, string Message)> _pendingEmails = [];

        public ReimbursementService(
            IReimbursementRepository reimbursementRepository,
            IUserRepository userRepository,
            IDepartmentRepository departmentRepository,
            IInventoryItemRepository inventoryItemRepository,
            INotificationRepository notificationRepository,
            IAuditLogRepository auditLogRepository,
            IBudgetService budgetService,
            IEmailSender emailSender,
            IUnitOfWork unitOfWork,
            ILogger<ReimbursementService> logger)
        {
            _reimbursementRepository = reimbursementRepository;
            _userRepository = userRepository;
            _departmentRepository = departmentRepository;
            _inventoryItemRepository = inventoryItemRepository;
            _notificationRepository = notificationRepository;
            _auditLogRepository = auditLogRepository;
            _budgetService = budgetService;
            _emailSender = emailSender;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // =====================================================================
        // Member: create, edit, submit, delete
        // =====================================================================

        public async Task<BaseResponse<ReimbursementRequestDto>> CreateAsync(CreateReimbursementRequestModel model, Guid userId)
        {
            try
            {
                var error = ValidateFields(model.PayeeName, model.MinisterInCharge, model.Items, out var items);
                if (error != null)
                {
                    return Fail<ReimbursementRequestDto>(error);
                }

                var classification = await ResolveClassificationAsync(model.Classification, model.DepartmentId, null);
                if (classification.Error != null)
                {
                    return Fail<ReimbursementRequestDto>(classification.Error);
                }

                if (!TryBuildReceipts(model.Receipts, userId, out var receipts, out var receiptError))
                {
                    return Fail<ReimbursementRequestDto>(receiptError!);
                }

                var request = new ReimbursementRequest
                {
                    RequestNumber = await GenerateRequestNumberAsync(),
                    PayeeUserId = userId,
                    PayeeName = NameNormalizer.Clean(model.PayeeName),
                    Classification = classification.Classification,
                    DepartmentId = classification.DepartmentId,
                    Department = classification.DisplayName,
                    MinisterInCharge = NameNormalizer.Clean(model.MinisterInCharge),
                    RequestDate = Validator.AsUtc(model.RequestDate),
                    Remarks = model.Remarks?.Trim(),
                    Status = ReimbursementStatus.Draft,
                    Items = items
                };

                foreach (var item in request.Items)
                {
                    item.ReimbursementRequestId = request.Id;
                }

                foreach (var receipt in receipts)
                {
                    receipt.ReimbursementRequestId = request.Id;
                    request.Receipts.Add(receipt);
                }

                ApplyTotals(request);
                await AddHistoryAsync(request, userId, "Created", null, ReimbursementStatus.Draft, "Draft created.");

                await _reimbursementRepository.CreateAsync(request);
                await _auditLogRepository.CreateAsync(NewAudit(request.Id, "Create", userId));
                await _unitOfWork.SaveChangesAsync();

                return await BuildResponseAsync(request, "Reimbursement draft saved.");
            }
            catch (Exception ex)
            {
                return Unexpected<ReimbursementRequestDto>(ex, "create the reimbursement");
            }
        }

        public async Task<BaseResponse<ReimbursementRequestDto>> UpdateDraftAsync(UpdateReimbursementRequestModel model, Guid userId)
        {
            try
            {
                var request = await _reimbursementRepository.GetWithDetailsAsync(model.Id);
                if (request == null)
                {
                    return Fail<ReimbursementRequestDto>("Reimbursement not found.");
                }

                if (request.PayeeUserId != userId)
                {
                    return Fail<ReimbursementRequestDto>("You can only edit your own reimbursement.");
                }

                if (request.Status is not (ReimbursementStatus.Draft or ReimbursementStatus.ReturnedForCorrection))
                {
                    return Fail<ReimbursementRequestDto>("Only drafts and requests returned for correction can be edited.");
                }

                var error = ValidateFields(model.PayeeName, model.MinisterInCharge, model.Items, out var items);
                if (error != null)
                {
                    return Fail<ReimbursementRequestDto>(error);
                }

                var classification = await ResolveClassificationAsync(model.Classification, model.DepartmentId, request.DepartmentId);
                if (classification.Error != null)
                {
                    return Fail<ReimbursementRequestDto>(classification.Error);
                }

                if (!TryBuildReceipts(model.Receipts, userId, out var receipts, out var receiptError))
                {
                    return Fail<ReimbursementRequestDto>(receiptError!);
                }

                request.PayeeName = NameNormalizer.Clean(model.PayeeName);
                request.Classification = classification.Classification;
                request.DepartmentId = classification.DepartmentId;
                request.Department = classification.DisplayName;
                request.MinisterInCharge = NameNormalizer.Clean(model.MinisterInCharge);
                request.RequestDate = Validator.AsUtc(model.RequestDate);
                request.Remarks = model.Remarks?.Trim();

                // Items are rebuilt on every edit. A physical item the Inventory Officer already
                // verified keeps that result (and its inventory record) if it is unchanged, so
                // editing the request does not create a duplicate inventory entry.
                var previous = request.Items.ToList();
                request.Items.Clear();
                foreach (var item in items)
                {
                    var match = previous.FirstOrDefault(p =>
                        p.ItemType == ReimbursementItemType.Physical &&
                        item.ItemType == ReimbursementItemType.Physical &&
                        p.VerificationStatus == InventoryVerificationStatus.Verified &&
                        string.Equals(p.ItemName, item.ItemName, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(p.SerialNumber ?? string.Empty, item.SerialNumber ?? string.Empty, StringComparison.OrdinalIgnoreCase) &&
                        p.Total == item.Total);
                    if (match != null)
                    {
                        previous.Remove(match);
                        item.VerificationStatus = match.VerificationStatus;
                        item.VerifiedBy = match.VerifiedBy;
                        item.VerifiedByName = match.VerifiedByName;
                        item.VerifiedDate = match.VerifiedDate;
                        item.VerificationNotes = match.VerificationNotes;
                        item.InventoryItemId = match.InventoryItemId;
                    }

                    item.ReimbursementRequestId = request.Id;
                    request.Items.Add(item);
                }

                _reimbursementRepository.TrackRangeAsAdded(items);

                foreach (var receipt in receipts)
                {
                    receipt.ReimbursementRequestId = request.Id;
                    request.Receipts.Add(receipt);
                }

                _reimbursementRepository.TrackRangeAsAdded(receipts);

                ApplyTotals(request);
                await AddHistoryAsync(request, userId, "Updated", null, null, "Request updated by the member.");
                await _auditLogRepository.CreateAsync(NewAudit(request.Id, "Update", userId));

                var saveError = await TrySaveAsync();
                if (saveError != null)
                {
                    return Fail<ReimbursementRequestDto>(saveError);
                }

                return await BuildResponseAsync(request, "Reimbursement updated.");
            }
            catch (Exception ex)
            {
                return Unexpected<ReimbursementRequestDto>(ex, "update the reimbursement");
            }
        }

        public async Task<BaseResponse<ReimbursementRequestDto>> SubmitAsync(Guid id, Guid userId)
        {
            try
            {
                var request = await _reimbursementRepository.GetWithDetailsAsync(id);
                if (request == null)
                {
                    return Fail<ReimbursementRequestDto>("Reimbursement not found.");
                }

                if (request.PayeeUserId != userId)
                {
                    return Fail<ReimbursementRequestDto>("You can only submit your own reimbursement.");
                }

                if (request.Status is not (ReimbursementStatus.Draft or ReimbursementStatus.ReturnedForCorrection))
                {
                    return Fail<ReimbursementRequestDto>("Only drafts and requests returned for correction can be submitted.");
                }

                var member = await _userRepository.GetByIdAsync(userId);
                if (member == null)
                {
                    return Fail<ReimbursementRequestDto>("We could not find your account.");
                }

                if (member.IsSelfRegistered && !member.EmailVerified)
                {
                    return Fail<ReimbursementRequestDto>(
                        "Please verify your email address before submitting. Use the link in your welcome email, or resend it from the banner at the top of the page.");
                }

                if (request.Classification == ReimbursementClassification.Department)
                {
                    if (request.DepartmentId == null)
                    {
                        return Fail<ReimbursementRequestDto>("Choose a department (or Personal Use) before submitting.");
                    }

                    var department = await _departmentRepository.GetByIdAsync(request.DepartmentId.Value);
                    if (department == null || !department.IsActive)
                    {
                        return Fail<ReimbursementRequestDto>(
                            "The department on this request is no longer accepting new requests. Edit it and choose another department.");
                    }

                    request.Department = department.Name;
                }

                if (request.Items.Count == 0)
                {
                    return Fail<ReimbursementRequestDto>("Add at least one item before submitting.");
                }

                if (!request.Receipts.Any(r => !r.IsDeleted))
                {
                    return Fail<ReimbursementRequestDto>("Attach at least one invoice or receipt before submitting. A reimbursement is void without a receipt.");
                }

                // Physical items go to the Inventory Officer. Anything not already verified
                // (new, or rejected before the request was returned) is checked again.
                var toVerify = request.Items
                    .Where(i => !i.IsDeleted && i.ItemType == ReimbursementItemType.Physical && i.VerificationStatus != InventoryVerificationStatus.Verified)
                    .ToList();
                foreach (var item in toVerify)
                {
                    item.VerificationStatus = InventoryVerificationStatus.Pending;
                    item.VerifiedBy = null;
                    item.VerifiedByName = null;
                    item.VerifiedDate = null;
                    item.VerificationNotes = null;
                }

                var fromStatus = request.Status;
                var resubmission = fromStatus == ReimbursementStatus.ReturnedForCorrection;
                request.Status = ReimbursementStatus.UnderReview;
                request.SubmittedDate ??= DateTime.UtcNow;
                request.InfoRequestNotes = null;

                await AddHistoryAsync(
                    request,
                    userId,
                    resubmission ? "Resubmitted" : "Submitted",
                    fromStatus,
                    ReimbursementStatus.UnderReview,
                    resubmission ? "Corrected and resubmitted for review." : "Submitted for review.");

                var officers = await _userRepository.GetUsersByRoleAsync(AccountsOfficerRole);
                await QueueNotifyAsync(
                    officers,
                    userId,
                    $"Reimbursement {request.RequestNumber} ready for review",
                    $"{request.PayeeName} {(resubmission ? "resubmitted" : "submitted")} {FormatMoney(request.TotalAmount)} ({request.Department}).",
                    request.Id);

                if (toVerify.Count > 0)
                {
                    var inventoryOfficers = await _userRepository.GetUsersByRoleAsync(InventoryOfficerRole);
                    var names = string.Join(", ", toVerify.Select(i => $"\"{i.ItemName}\""));
                    await QueueNotifyAsync(
                        inventoryOfficers,
                        userId,
                        $"Item{(toVerify.Count == 1 ? "" : "s")} to confirm on {request.RequestNumber}",
                        $"{request.PayeeName} submitted a reimbursement with {toVerify.Count} physical item{(toVerify.Count == 1 ? "" : "s")} " +
                        $"({names}) that need{(toVerify.Count == 1 ? "s" : "")} to be confirmed and verified. " +
                        "Open Items to Verify on your Inventory dashboard to verify or reject them.",
                        request.Id);
                }

                await _auditLogRepository.CreateAsync(NewAudit(request.Id, resubmission ? "Resubmit" : "Submit", userId));

                var saveError = await TrySaveAsync();
                if (saveError != null)
                {
                    return Fail<ReimbursementRequestDto>(saveError);
                }

                await FlushEmailsAsync();
                return await BuildResponseAsync(
                    request,
                    toVerify.Count > 0
                        ? "Submitted. Your request is under review, and the Inventory Officer has been asked to verify your physical items."
                        : "Submitted. Your request is now under review.");
            }
            catch (Exception ex)
            {
                return Unexpected<ReimbursementRequestDto>(ex, "submit the reimbursement");
            }
        }

        public async Task<BaseResponse<bool>> DeleteDraftAsync(Guid id, Guid userId)
        {
            try
            {
                var request = await _reimbursementRepository.GetWithDetailsAsync(id);
                if (request == null)
                {
                    return Fail<bool>("Reimbursement not found.");
                }

                if (request.PayeeUserId != userId)
                {
                    return Fail<bool>("You can only delete your own draft.");
                }

                if (request.Status != ReimbursementStatus.Draft)
                {
                    return Fail<bool>("Only drafts can be deleted.");
                }

                request.IsDeleted = true;
                await _auditLogRepository.CreateAsync(NewAudit(request.Id, "Delete", userId));

                var saveError = await TrySaveAsync();
                if (saveError != null)
                {
                    return Fail<bool>(saveError);
                }

                return new BaseResponse<bool> { Message = "Draft deleted.", Status = true, Data = true };
            }
            catch (Exception ex)
            {
                return Unexpected<bool>(ex, "delete the draft");
            }
        }

        // =====================================================================
        // Stage two: Accounts Officer review
        // =====================================================================

        public async Task<BaseResponse<ReimbursementRequestDto>> ReviewAsync(
            Guid id, ReviewReimbursementModel model, Guid userId, IReadOnlyCollection<string> roles)
        {
            try
            {
                if (!roles.Contains(AccountsOfficerRole))
                {
                    return Fail<ReimbursementRequestDto>("Only an Accounts Officer can review a request.");
                }

                var request = await _reimbursementRepository.GetWithDetailsAsync(id);
                if (request == null)
                {
                    return Fail<ReimbursementRequestDto>("Reimbursement not found.");
                }

                if (request.PayeeUserId == userId)
                {
                    return Fail<ReimbursementRequestDto>("You cannot review your own reimbursement.");
                }

                if (!IsAwaitingReview(request.Status))
                {
                    return Fail<ReimbursementRequestDto>($"Only requests under review can be forwarded. This one is {Describe(request.Status)}.");
                }

                if (request.Classification == ReimbursementClassification.Department && request.DepartmentId == null)
                {
                    return Fail<ReimbursementRequestDto>(
                        $"This older request still has the typed department \"{request.Department}\". " +
                        "Ask an Accounts Admin to link it to a department, or return it to the member.");
                }

                var inventoryBlock = InventoryBlockMessage(request);
                if (inventoryBlock != null)
                {
                    return Fail<ReimbursementRequestDto>(
                        inventoryBlock + " You can return or reject the request; only an Accounts Admin can approve it with an override.");
                }

                var budget = await _budgetService.GetRequestBudgetAsync(request);
                var notes = model.Notes?.Trim();
                var historyNote = string.IsNullOrWhiteSpace(notes) ? "Reviewed and forwarded for final approval." : notes;
                if (budget.RequiresOverBudgetJustification)
                {
                    historyNote += " [Budget warning at review: " + string.Join(" ", budget.Alerts.Where(a => a.Severity != "info").Select(a => a.Message)) + "]";
                }

                var fromStatus = request.Status;
                request.Status = ReimbursementStatus.PendingApproval;
                request.ReviewedBy = userId;
                request.ReviewedDate = DateTime.UtcNow;
                request.ReviewNotes = notes;

                await AddHistoryAsync(request, userId, "Reviewed", fromStatus, ReimbursementStatus.PendingApproval, historyNote);

                var admins = await _userRepository.GetUsersByRoleAsync(AccountsAdminRole);
                await QueueNotifyAsync(
                    admins,
                    userId,
                    $"Reimbursement {request.RequestNumber} awaiting final approval",
                    $"{request.PayeeName}'s request for {FormatMoney(request.TotalAmount)} ({request.Department}) passed accounts review." +
                    (budget.RequiresOverBudgetJustification ? " It is over budget." : string.Empty),
                    request.Id);
                await QueueNotifyPayeeAsync(
                    request,
                    userId,
                    $"Reimbursement {request.RequestNumber} passed review",
                    "Your request passed accounts review and is waiting for final approval. It has not been approved or paid yet.");

                await _auditLogRepository.CreateAsync(NewAudit(request.Id, "Review", userId));

                var saveError = await TrySaveAsync();
                if (saveError != null)
                {
                    return Fail<ReimbursementRequestDto>(saveError);
                }

                await FlushEmailsAsync();
                return await BuildResponseAsync(request, "Forwarded for final approval.");
            }
            catch (Exception ex)
            {
                return Unexpected<ReimbursementRequestDto>(ex, "review the reimbursement");
            }
        }

        public async Task<BaseResponse<ReimbursementRequestDto>> ReturnForCorrectionAsync(
            Guid id, RequestReimbursementInfoModel model, Guid userId, IReadOnlyCollection<string> roles)
        {
            try
            {
                if (Validator.CheckString(model.Notes))
                {
                    return Fail<ReimbursementRequestDto>("Explain what the member needs to correct or add.");
                }

                var request = await _reimbursementRepository.GetWithDetailsAsync(id);
                if (request == null)
                {
                    return Fail<ReimbursementRequestDto>("Reimbursement not found.");
                }

                var stageError = CheckStageActor(request, userId, roles, "return");
                if (stageError != null)
                {
                    return Fail<ReimbursementRequestDto>(stageError);
                }

                var notes = model.Notes.Trim();
                var fromStatus = request.Status;
                request.Status = ReimbursementStatus.ReturnedForCorrection;
                request.InfoRequestNotes = notes;

                await AddHistoryAsync(request, userId, "ReturnedForCorrection", fromStatus, ReimbursementStatus.ReturnedForCorrection, notes);
                await QueueNotifyPayeeAsync(request, userId, $"Correction needed on {request.RequestNumber}", notes);
                await _auditLogRepository.CreateAsync(NewAudit(request.Id, "ReturnForCorrection", userId));

                var saveError = await TrySaveAsync();
                if (saveError != null)
                {
                    return Fail<ReimbursementRequestDto>(saveError);
                }

                await FlushEmailsAsync();
                return await BuildResponseAsync(request, "Returned to the member for correction.");
            }
            catch (Exception ex)
            {
                return Unexpected<ReimbursementRequestDto>(ex, "return the reimbursement");
            }
        }

        public async Task<BaseResponse<ReimbursementRequestDto>> RejectAsync(
            Guid id, RejectReimbursementModel model, Guid userId, IReadOnlyCollection<string> roles)
        {
            try
            {
                if (Validator.CheckString(model.Reason))
                {
                    return Fail<ReimbursementRequestDto>("A rejection reason is required.");
                }

                var request = await _reimbursementRepository.GetWithDetailsAsync(id);
                if (request == null)
                {
                    return Fail<ReimbursementRequestDto>("Reimbursement not found.");
                }

                var stageError = CheckStageActor(request, userId, roles, "reject");
                if (stageError != null)
                {
                    return Fail<ReimbursementRequestDto>(stageError);
                }

                var reason = model.Reason.Trim();
                var fromStatus = request.Status;
                request.Status = ReimbursementStatus.Rejected;
                request.RejectedBy = userId;
                request.RejectedDate = DateTime.UtcNow;
                request.RejectionReason = reason;

                await AddHistoryAsync(request, userId, "Rejected", fromStatus, ReimbursementStatus.Rejected, reason);
                await QueueNotifyPayeeAsync(request, userId, $"Reimbursement {request.RequestNumber} rejected", reason);
                await _auditLogRepository.CreateAsync(NewAudit(request.Id, "Reject", userId));

                var saveError = await TrySaveAsync();
                if (saveError != null)
                {
                    return Fail<ReimbursementRequestDto>(saveError);
                }

                await FlushEmailsAsync();
                return await BuildResponseAsync(request, "Reimbursement rejected.");
            }
            catch (Exception ex)
            {
                return Unexpected<ReimbursementRequestDto>(ex, "reject the reimbursement");
            }
        }

        // =====================================================================
        // Stage three: Accounts Admin final approval and payment
        // =====================================================================

        public async Task<BaseResponse<ReimbursementRequestDto>> ApproveAsync(
            Guid id, ReimbursementDecisionModel model, Guid userId, IReadOnlyCollection<string> roles)
        {
            try
            {
                if (!roles.Contains(AccountsAdminRole))
                {
                    return Fail<ReimbursementRequestDto>("Only an Accounts Admin can give final approval.");
                }

                var request = await _reimbursementRepository.GetWithDetailsAsync(id);
                if (request == null)
                {
                    return Fail<ReimbursementRequestDto>("Reimbursement not found.");
                }

                if (request.PayeeUserId == userId)
                {
                    return Fail<ReimbursementRequestDto>("You cannot approve your own reimbursement.");
                }

                // The Accounts Admin can approve straight from Under Review; the officer's
                // forward is helpful but not required.
                var fromStatus = request.Status;
                var direct = IsAwaitingReview(fromStatus);
                if (!direct && fromStatus != ReimbursementStatus.PendingApproval)
                {
                    return Fail<ReimbursementRequestDto>(
                        $"Only requests that are under review or pending approval can be approved. This one is {Describe(request.Status)}.");
                }

                if (!direct && request.ReviewedBy == userId)
                {
                    return Fail<ReimbursementRequestDto>("The person who reviewed a request cannot also give its final approval.");
                }

                if (request.Classification == ReimbursementClassification.Department && request.DepartmentId == null)
                {
                    return Fail<ReimbursementRequestDto>(
                        $"This older request still has the typed department \"{request.Department}\". Link it to a department first (Departments → Needs linking).");
                }

                // Physical items not confirmed by the Inventory Officer need a written override.
                var inventoryBlock = InventoryBlockMessage(request);
                var overrideReason = model.InventoryOverrideReason?.Trim();
                if (inventoryBlock != null && string.IsNullOrWhiteSpace(overrideReason))
                {
                    return Fail<ReimbursementRequestDto>(inventoryBlock + " To approve anyway, enter an inventory override reason.");
                }

                var approvedAmount = model.ApprovedAmount ?? request.TotalAmount;
                approvedAmount = Math.Round(approvedAmount, 2, MidpointRounding.AwayFromZero);
                if (approvedAmount <= 0)
                {
                    return Fail<ReimbursementRequestDto>("The approved amount must be greater than zero.");
                }

                if (approvedAmount > request.TotalAmount)
                {
                    return Fail<ReimbursementRequestDto>($"The approved amount cannot be more than the requested total of {FormatMoney(request.TotalAmount)}.");
                }

                var notes = model.Notes?.Trim();
                if (approvedAmount < request.TotalAmount && string.IsNullOrWhiteSpace(notes))
                {
                    return Fail<ReimbursementRequestDto>("Add a note explaining why less than the requested total is being approved.");
                }

                // Budget is checked on the server at the moment of approval. Going over
                // budget is allowed, but only with a recorded justification.
                var budget = await _budgetService.GetRequestBudgetAsync(request, approvedAmount);
                var justification = model.OverBudgetJustification?.Trim();
                if (budget.RequiresOverBudgetJustification && string.IsNullOrWhiteSpace(justification))
                {
                    return Fail<ReimbursementRequestDto>(
                        "This approval would exceed the department's budget. Enter a justification to approve it over budget.");
                }

                request.Status = ReimbursementStatus.Approved;
                request.ApprovedBy = userId;
                request.ApprovedDate = DateTime.UtcNow;
                request.ApprovedAmount = approvedAmount;
                request.ApprovalNotes = notes;
                request.OverBudgetAtApproval = budget.RequiresOverBudgetJustification;
                request.OverBudgetJustification = budget.RequiresOverBudgetJustification ? justification : null;
                request.InventoryOverride = inventoryBlock != null;
                request.InventoryOverrideReason = inventoryBlock != null ? overrideReason : null;
                request.PreparedBy = model.PreparedBy?.Trim() ?? request.PreparedBy;
                request.ChequeNumber = model.ChequeNumber?.Trim() ?? request.ChequeNumber;
                request.AuthorizedByName1 = model.AuthorizedByName1?.Trim() ?? request.AuthorizedByName1;
                request.AuthorizedByName2 = model.AuthorizedByName2?.Trim() ?? request.AuthorizedByName2;
                request.AuthorizationDate = Validator.AsUtc(model.AuthorizationDate) ?? request.AuthorizationDate;

                var historyNote = string.IsNullOrWhiteSpace(notes) ? "Final approval given." : notes;
                historyNote += $" Approved amount {FormatMoney(approvedAmount)}.";
                if (direct)
                {
                    historyNote += " Approved directly from Under Review.";
                }

                if (budget.RequiresOverBudgetJustification)
                {
                    historyNote += $" Approved over budget. Justification: {justification}";
                }

                if (inventoryBlock != null)
                {
                    historyNote += $" Inventory override ({inventoryBlock}) Reason: {overrideReason}";
                }

                await AddHistoryAsync(request, userId, "Approved", fromStatus, ReimbursementStatus.Approved, historyNote);
                await QueueNotifyPayeeAsync(
                    request,
                    userId,
                    $"Reimbursement {request.RequestNumber} approved",
                    $"Your request was approved for {FormatMoney(approvedAmount)}. You will be notified again when payment is made.");
                await QueueNotifyReviewerAsync(
                    request,
                    userId,
                    $"Reimbursement {request.RequestNumber} approved",
                    $"{request.PayeeName}'s request was approved for {FormatMoney(approvedAmount)} and is awaiting payment.");
                var auditDetails = new List<string>();
                if (direct) auditDetails.Add("Approved directly from Under Review.");
                if (budget.RequiresOverBudgetJustification) auditDetails.Add($"Over budget. Justification: {justification}");
                if (inventoryBlock != null) auditDetails.Add($"Inventory override. Reason: {overrideReason}");
                await _auditLogRepository.CreateAsync(NewAudit(request.Id, "Approve", userId,
                    auditDetails.Count > 0 ? string.Join(" ", auditDetails) : null));

                var saveError = await TrySaveAsync();
                if (saveError != null)
                {
                    return Fail<ReimbursementRequestDto>(saveError);
                }

                await FlushEmailsAsync();
                return await BuildResponseAsync(request, "Approved. Payment has not been recorded yet.");
            }
            catch (Exception ex)
            {
                return Unexpected<ReimbursementRequestDto>(ex, "approve the reimbursement");
            }
        }

        public async Task<BaseResponse<ReimbursementRequestDto>> RecordPaymentAsync(
            Guid id, RecordReimbursementPaymentModel model, Guid userId, IReadOnlyCollection<string> roles)
        {
            try
            {
                if (!roles.Contains(AccountsAdminRole))
                {
                    return Fail<ReimbursementRequestDto>("Only an Accounts Admin can confirm payment.");
                }

                var request = await _reimbursementRepository.GetWithDetailsAsync(id);
                if (request == null)
                {
                    return Fail<ReimbursementRequestDto>("Reimbursement not found.");
                }

                if (request.PayeeUserId == userId)
                {
                    return Fail<ReimbursementRequestDto>("You cannot confirm payment of your own reimbursement.");
                }

                if (request.Status == ReimbursementStatus.Paid)
                {
                    return Fail<ReimbursementRequestDto>("Payment for this request has already been confirmed.");
                }

                if (request.Status != ReimbursementStatus.Approved)
                {
                    return Fail<ReimbursementRequestDto>($"Payment can only be confirmed after final approval. This request is {Describe(request.Status)}.");
                }

                var amount = Math.Round(model.AmountPaid, 2, MidpointRounding.AwayFromZero);
                var committed = request.ApprovedAmount ?? request.TotalAmount;

                if (amount <= 0)
                {
                    return Fail<ReimbursementRequestDto>("The amount paid must be greater than zero.");
                }

                if (amount > committed)
                {
                    return Fail<ReimbursementRequestDto>($"The amount paid cannot be more than the approved amount of {FormatMoney(committed)}.");
                }

                if (Validator.CheckString(model.PaymentMethod))
                {
                    return Fail<ReimbursementRequestDto>("Payment method is required.");
                }

                if (amount != committed && string.IsNullOrWhiteSpace(model.FinanceNotes))
                {
                    return Fail<ReimbursementRequestDto>("Add a finance note explaining the difference between the approved amount and the amount paid.");
                }

                var paymentDate = model.PaymentDate == default ? DateTime.UtcNow : Validator.AsUtc(model.PaymentDate);
                if (paymentDate.Date > DateTime.UtcNow.Date.AddDays(1))
                {
                    return Fail<ReimbursementRequestDto>("The payment date cannot be in the future.");
                }

                if (request.ApprovedDate.HasValue && paymentDate.Date < request.ApprovedDate.Value.Date)
                {
                    return Fail<ReimbursementRequestDto>("The payment date cannot be before the approval date.");
                }

                request.Status = ReimbursementStatus.Paid;
                request.PaidBy = userId;
                request.PaymentDate = paymentDate;
                request.PaymentConfirmedAt = DateTime.UtcNow;
                request.AmountPaid = amount;
                request.PaymentMethod = model.PaymentMethod.Trim();
                request.PaymentReference = model.PaymentReference?.Trim();
                request.PreparedBy = model.PreparedBy?.Trim() ?? request.PreparedBy;
                request.ChequeNumber = model.ChequeNumber?.Trim() ?? request.ChequeNumber;
                request.AuthorizedByName1 = model.AuthorizedByName1?.Trim() ?? request.AuthorizedByName1;
                request.AuthorizedByName2 = model.AuthorizedByName2?.Trim() ?? request.AuthorizedByName2;
                request.AuthorizationDate = Validator.AsUtc(model.AuthorizationDate) ?? request.AuthorizationDate;
                request.FinanceNotes = model.FinanceNotes?.Trim();

                var note = $"Paid {FormatMoney(amount)} via {request.PaymentMethod} on {paymentDate:yyyy-MM-dd}.";
                if (!string.IsNullOrWhiteSpace(request.PaymentReference))
                {
                    note += $" Reference {request.PaymentReference}.";
                }

                await AddHistoryAsync(request, userId, "Paid", ReimbursementStatus.Approved, ReimbursementStatus.Paid, note);
                await QueueNotifyPayeeAsync(
                    request,
                    userId,
                    $"Reimbursement {request.RequestNumber} paid",
                    $"{FormatMoney(amount)} was paid via {request.PaymentMethod}.");
                await QueueNotifyReviewerAsync(
                    request,
                    userId,
                    $"Reimbursement {request.RequestNumber} paid",
                    $"{request.PayeeName}'s reimbursement of {FormatMoney(amount)} was paid.");
                await _auditLogRepository.CreateAsync(NewAudit(request.Id, "ConfirmPayment", userId, note));

                var saveError = await TrySaveAsync();
                if (saveError != null)
                {
                    return Fail<ReimbursementRequestDto>(saveError);
                }

                await FlushEmailsAsync();
                return await BuildResponseAsync(request, "Payment confirmed.");
            }
            catch (Exception ex)
            {
                return Unexpected<ReimbursementRequestDto>(ex, "confirm the payment");
            }
        }

        // =====================================================================
        // Inventory Officer: verify physical items
        // =====================================================================

        private static bool CanVerifyAt(ReimbursementStatus status) =>
            status is ReimbursementStatus.UnderReview or ReimbursementStatus.LegacyAccountsReview
                or ReimbursementStatus.PendingApproval or ReimbursementStatus.Approved or ReimbursementStatus.Paid;

        public async Task<BaseResponse<ICollection<InventoryVerificationItemDto>>> GetVerificationQueueAsync(
            bool pendingOnly, Guid userId, IReadOnlyCollection<string> roles)
        {
            try
            {
                if (!roles.Contains(InventoryOfficerRole) && !IsPrivileged(roles))
                {
                    return Fail<ICollection<InventoryVerificationItemDto>>("Only the Inventory Officer and Accounts staff can see items to verify.");
                }

                var requests = await _reimbursementRepository.GetWithPhysicalItemsAsync(pendingOnly);
                var receipts = await _reimbursementRepository.GetReceiptMetadataAsync(requests.Select(r => r.Id).ToList());

                var data = new List<InventoryVerificationItemDto>();
                foreach (var request in requests)
                {
                    var requestReceipts = (receipts.TryGetValue(request.Id, out var list) ? list : [])
                        .Where(r => !r.IsDeleted)
                        .Select(r => new ReimbursementReceiptDto
                        {
                            Id = r.Id,
                            FileName = r.FileName,
                            ContentType = r.ContentType,
                            FileSize = r.FileSize,
                            UploadedBy = r.UploadedBy,
                            CreatedDate = r.CreatedDate
                        })
                        .ToList();

                    var department = request.Classification == ReimbursementClassification.PersonalUse
                        ? PersonalUseLabel
                        : request.DepartmentRecord?.Name ?? request.Department;

                    foreach (var item in request.Items
                        .Where(i => !i.IsDeleted && i.ItemType == ReimbursementItemType.Physical)
                        .Where(i => pendingOnly ? i.VerificationStatus == InventoryVerificationStatus.Pending : i.VerificationStatus != InventoryVerificationStatus.NotRequired)
                        .OrderBy(i => i.ReferenceNumber))
                    {
                        data.Add(new InventoryVerificationItemDto
                        {
                            ItemId = item.Id,
                            ReimbursementRequestId = request.Id,
                            RequestNumber = request.RequestNumber,
                            RequestStatus = request.Status,
                            PayeeName = request.PayeeName,
                            Department = department,
                            RequestDate = request.RequestDate,
                            SubmittedDate = request.SubmittedDate,
                            ReferenceNumber = item.ReferenceNumber,
                            ItemName = item.ItemName,
                            SerialNumber = item.SerialNumber,
                            Total = item.Total,
                            VerificationStatus = item.VerificationStatus,
                            VerifiedByName = item.VerifiedByName,
                            VerifiedDate = item.VerifiedDate,
                            VerificationNotes = item.VerificationNotes,
                            InventoryItemId = item.InventoryItemId,
                            Receipts = requestReceipts
                        });
                    }
                }

                return new BaseResponse<ICollection<InventoryVerificationItemDto>> { Message = "Items found.", Status = true, Data = data };
            }
            catch (Exception ex)
            {
                return Unexpected<ICollection<InventoryVerificationItemDto>>(ex, "load items to verify");
            }
        }

        public async Task<BaseResponse<InventoryVerificationItemDto>> VerifyItemAsync(
            Guid itemId, VerifyReimbursementItemModel model, Guid userId, IReadOnlyCollection<string> roles)
        {
            try
            {
                if (!roles.Contains(InventoryOfficerRole))
                {
                    return Fail<InventoryVerificationItemDto>("Only an Inventory Officer can verify items.");
                }

                var request = await _reimbursementRepository.GetByItemIdAsync(itemId);
                var item = request?.Items.FirstOrDefault(i => i.Id == itemId && !i.IsDeleted);
                if (request == null || item == null)
                {
                    return Fail<InventoryVerificationItemDto>("Item not found.");
                }

                if (request.PayeeUserId == userId)
                {
                    return Fail<InventoryVerificationItemDto>("You cannot verify items on your own reimbursement.");
                }

                if (item.ItemType != ReimbursementItemType.Physical)
                {
                    return Fail<InventoryVerificationItemDto>("Only physical items are verified.");
                }

                if (!CanVerifyAt(request.Status))
                {
                    return Fail<InventoryVerificationItemDto>($"This request is {Describe(request.Status)}, so its items can't be verified now.");
                }

                if (item.VerificationStatus == InventoryVerificationStatus.Verified)
                {
                    return Fail<InventoryVerificationItemDto>("This item has already been verified and added to inventory.");
                }

                if (Validator.CheckString(model.ItemName)) return Fail<InventoryVerificationItemDto>("Item name is required.");
                if (Validator.CheckString(model.Category)) return Fail<InventoryVerificationItemDto>("Category is required.");
                if (Validator.CheckString(model.Location)) return Fail<InventoryVerificationItemDto>("Location is required.");
                if (Validator.CheckString(model.Custodian)) return Fail<InventoryVerificationItemDto>("Custodian is required.");
                if (Validator.CheckString(model.Description)) return Fail<InventoryVerificationItemDto>("Description is required.");
                if (model.Quantity < 1) return Fail<InventoryVerificationItemDto>("Quantity must be at least 1.");
                if (model.Value < 0) return Fail<InventoryVerificationItemDto>("Value cannot be negative.");
                if (!Enum.IsDefined(model.Condition)) return Fail<InventoryVerificationItemDto>("Choose the item's condition.");

                var now = DateTime.UtcNow;
                var serial = model.SerialNumber?.Trim();
                var inventory = new InventoryItem
                {
                    ItemName = model.ItemName.Trim(),
                    Category = model.Category.Trim(),
                    Quantity = model.Quantity,
                    Location = model.Location.Trim(),
                    Condition = model.Condition,
                    PurchaseDate = model.PurchaseDate == default ? request.RequestDate : Validator.AsUtc(model.PurchaseDate),
                    Value = Math.Round(model.Value, 2, MidpointRounding.AwayFromZero),
                    SerialNumber = string.IsNullOrWhiteSpace(serial) ? null : serial,
                    Description = model.Description.Trim(),
                    Custodian = model.Custodian.Trim(),
                    LastVerifiedDate = now
                };
                await _inventoryItemRepository.CreateAsync(inventory);

                var names = await _userRepository.GetDisplayNamesAsync([userId]);
                var officerName = names.TryGetValue(userId, out var n) ? n : "the Inventory Officer";
                var notes = model.Notes?.Trim();

                item.VerificationStatus = InventoryVerificationStatus.Verified;
                item.VerifiedBy = userId;
                item.VerifiedByName = officerName;
                item.VerifiedDate = now;
                item.VerificationNotes = string.IsNullOrWhiteSpace(notes) ? null : notes;
                item.InventoryItemId = inventory.Id;
                if (string.IsNullOrWhiteSpace(item.SerialNumber) && inventory.SerialNumber != null)
                {
                    item.SerialNumber = inventory.SerialNumber;
                }

                var remaining = request.Items.Count(i => !i.IsDeleted && i.ItemType == ReimbursementItemType.Physical &&
                    i.VerificationStatus != InventoryVerificationStatus.Verified);

                await AddHistoryAsync(request, userId, "ItemVerified", null, null,
                    $"Inventory Officer verified \"{item.ItemName}\" and added it to inventory." + (item.VerificationNotes != null ? $" {item.VerificationNotes}" : ""));

                var accounts = (await _userRepository.GetUsersByRoleAsync(AccountsOfficerRole))
                    .Concat(await _userRepository.GetUsersByRoleAsync(AccountsAdminRole));
                await QueueNotifyAsync(
                    accounts,
                    userId,
                    $"Item verified on {request.RequestNumber}",
                    $"{officerName} verified \"{item.ItemName}\" on {request.PayeeName}'s request and added it to inventory. " +
                    (remaining == 0
                        ? "All physical items on this request are now verified."
                        : $"{remaining} physical item{(remaining == 1 ? " is" : "s are")} still not verified."),
                    request.Id);

                await _auditLogRepository.CreateAsync(NewAudit(request.Id, "VerifyItem", userId, $"Item {item.Id}; inventory {inventory.Id}"));
                await _auditLogRepository.CreateAsync(new AuditLog
                {
                    EntityName = nameof(InventoryItem),
                    EntityId = inventory.Id,
                    Action = "Create",
                    UserId = userId,
                    Details = $"From reimbursement {request.RequestNumber}"
                });

                var saveError = await TrySaveAsync();
                if (saveError != null)
                {
                    return Fail<InventoryVerificationItemDto>(saveError);
                }

                await FlushEmailsAsync();
                return new BaseResponse<InventoryVerificationItemDto>
                {
                    Message = "Verified and added to inventory. The Accounts team has been notified.",
                    Status = true,
                    Data = ToVerificationDto(request, item)
                };
            }
            catch (Exception ex)
            {
                return Unexpected<InventoryVerificationItemDto>(ex, "verify the item");
            }
        }

        public async Task<BaseResponse<InventoryVerificationItemDto>> RejectItemAsync(
            Guid itemId, RejectReimbursementItemModel model, Guid userId, IReadOnlyCollection<string> roles)
        {
            try
            {
                if (!roles.Contains(InventoryOfficerRole))
                {
                    return Fail<InventoryVerificationItemDto>("Only an Inventory Officer can reject items.");
                }

                if (Validator.CheckString(model.Reason))
                {
                    return Fail<InventoryVerificationItemDto>("Give a reason for rejecting the item.");
                }

                var request = await _reimbursementRepository.GetByItemIdAsync(itemId);
                var item = request?.Items.FirstOrDefault(i => i.Id == itemId && !i.IsDeleted);
                if (request == null || item == null)
                {
                    return Fail<InventoryVerificationItemDto>("Item not found.");
                }

                if (request.PayeeUserId == userId)
                {
                    return Fail<InventoryVerificationItemDto>("You cannot reject items on your own reimbursement.");
                }

                if (item.ItemType != ReimbursementItemType.Physical)
                {
                    return Fail<InventoryVerificationItemDto>("Only physical items are verified.");
                }

                if (!CanVerifyAt(request.Status))
                {
                    return Fail<InventoryVerificationItemDto>($"This request is {Describe(request.Status)}, so its items can't be checked now.");
                }

                if (item.VerificationStatus == InventoryVerificationStatus.Verified)
                {
                    return Fail<InventoryVerificationItemDto>("This item was already verified and added to inventory, so it can't be rejected.");
                }

                if (item.VerificationStatus == InventoryVerificationStatus.Rejected)
                {
                    return Fail<InventoryVerificationItemDto>("This item has already been rejected.");
                }

                var reason = model.Reason.Trim();
                var names = await _userRepository.GetDisplayNamesAsync([userId]);
                var officerName = names.TryGetValue(userId, out var n) ? n : "the Inventory Officer";

                item.VerificationStatus = InventoryVerificationStatus.Rejected;
                item.VerifiedBy = userId;
                item.VerifiedByName = officerName;
                item.VerifiedDate = DateTime.UtcNow;
                item.VerificationNotes = reason;

                await AddHistoryAsync(request, userId, "ItemRejected", null, null,
                    $"Inventory Officer rejected \"{item.ItemName}\". Reason: {reason}");

                var accounts = (await _userRepository.GetUsersByRoleAsync(AccountsOfficerRole))
                    .Concat(await _userRepository.GetUsersByRoleAsync(AccountsAdminRole));
                await QueueNotifyAsync(
                    accounts,
                    userId,
                    $"Item rejected on {request.RequestNumber}",
                    $"{officerName} could not verify \"{item.ItemName}\" on {request.PayeeName}'s request. Reason: {reason} " +
                    "The request can't be forwarded until this is resolved; an Accounts Admin can still approve it with an override.",
                    request.Id);
                await QueueNotifyPayeeAsync(
                    request,
                    userId,
                    $"An item on {request.RequestNumber} was not verified",
                    $"The Inventory Officer could not verify \"{item.ItemName}\". Reason: {reason} " +
                    "The Accounts team will contact you or return the request so you can correct it.");

                await _auditLogRepository.CreateAsync(NewAudit(request.Id, "RejectItem", userId, $"Item {item.Id}. Reason: {reason}"));

                var saveError = await TrySaveAsync();
                if (saveError != null)
                {
                    return Fail<InventoryVerificationItemDto>(saveError);
                }

                await FlushEmailsAsync();
                return new BaseResponse<InventoryVerificationItemDto>
                {
                    Message = "Item rejected. The Accounts team and the member have been notified.",
                    Status = true,
                    Data = ToVerificationDto(request, item)
                };
            }
            catch (Exception ex)
            {
                return Unexpected<InventoryVerificationItemDto>(ex, "reject the item");
            }
        }

        private static InventoryVerificationItemDto ToVerificationDto(ReimbursementRequest request, ReimbursementItem item) => new()
        {
            ItemId = item.Id,
            ReimbursementRequestId = request.Id,
            RequestNumber = request.RequestNumber,
            RequestStatus = request.Status,
            PayeeName = request.PayeeName,
            Department = request.Classification == ReimbursementClassification.PersonalUse
                ? PersonalUseLabel
                : request.DepartmentRecord?.Name ?? request.Department,
            RequestDate = request.RequestDate,
            SubmittedDate = request.SubmittedDate,
            ReferenceNumber = item.ReferenceNumber,
            ItemName = item.ItemName,
            SerialNumber = item.SerialNumber,
            Total = item.Total,
            VerificationStatus = item.VerificationStatus,
            VerifiedByName = item.VerifiedByName,
            VerifiedDate = item.VerifiedDate,
            VerificationNotes = item.VerificationNotes,
            InventoryItemId = item.InventoryItemId
        };

        // =====================================================================
        // Reading
        // =====================================================================

        public async Task<BaseResponse<ReimbursementRequestDto>> GetAsync(Guid id, Guid userId, IReadOnlyCollection<string> roles)
        {
            try
            {
                var request = await _reimbursementRepository.GetWithDetailsAsync(id);
                if (request == null)
                {
                    return Fail<ReimbursementRequestDto>("Reimbursement not found.");
                }

                if (!IsPrivileged(roles) && request.PayeeUserId != userId)
                {
                    // Same message as "not found" so ids of other people's requests are not confirmed.
                    return Fail<ReimbursementRequestDto>("Reimbursement not found.");
                }

                return await BuildResponseAsync(request, "Reimbursement found.");
            }
            catch (Exception ex)
            {
                return Unexpected<ReimbursementRequestDto>(ex, "load the reimbursement");
            }
        }

        public async Task<BaseResponse<ICollection<ReimbursementRequestDto>>> GetAllAsync(Guid userId, IReadOnlyCollection<string> roles)
        {
            try
            {
                var privileged = IsPrivileged(roles);
                var requests = privileged
                    // Officers and admins see submitted requests; other people's drafts stay private.
                    ? await _reimbursementRepository.GetAllWithDetailsAsync(r => r.Status != ReimbursementStatus.Draft || r.PayeeUserId == userId)
                    : await _reimbursementRepository.GetAllWithDetailsAsync(r => r.PayeeUserId == userId);

                var ids = requests.Select(r => r.Id).ToList();
                var receipts = await _reimbursementRepository.GetReceiptMetadataAsync(ids);
                var names = await _userRepository.GetDisplayNamesAsync(requests.SelectMany(ActorIds));
                var emails = await GetEmailsAsync(requests.Select(r => r.PayeeUserId));

                var data = requests
                    .Select(r => ToDto(
                        r,
                        emails.TryGetValue(r.PayeeUserId, out var email) ? email : null,
                        names,
                        receipts.TryGetValue(r.Id, out var list) ? list : []))
                    .ToList();

                return new BaseResponse<ICollection<ReimbursementRequestDto>> { Message = "Reimbursements found.", Status = true, Data = data };
            }
            catch (Exception ex)
            {
                return Unexpected<ICollection<ReimbursementRequestDto>>(ex, "load reimbursements");
            }
        }

        public async Task<BaseResponse<ReimbursementBudgetDto>> GetBudgetAsync(Guid id, decimal? amount, Guid userId, IReadOnlyCollection<string> roles)
        {
            try
            {
                if (!IsPrivileged(roles))
                {
                    return Fail<ReimbursementBudgetDto>("Only Accounts staff can see budget figures.");
                }

                var request = await _reimbursementRepository.GetWithDetailsAsync(id);
                if (request == null)
                {
                    return Fail<ReimbursementBudgetDto>("Reimbursement not found.");
                }

                var budget = await _budgetService.GetRequestBudgetAsync(request, amount);
                return new BaseResponse<ReimbursementBudgetDto> { Message = "Budget calculated.", Status = true, Data = budget };
            }
            catch (Exception ex)
            {
                return Unexpected<ReimbursementBudgetDto>(ex, "calculate the budget");
            }
        }

        public async Task<BaseResponse<ReceiptFileDto>> GetReceiptAsync(Guid receiptId, Guid userId, IReadOnlyCollection<string> roles)
        {
            try
            {
                var receipt = await _reimbursementRepository.GetReceiptAsync(receiptId);
                if (receipt == null)
                {
                    return Fail<ReceiptFileDto>("Receipt not found.");
                }

                var ownerId = await _reimbursementRepository.GetAllAsync(r => r.Id == receipt.ReimbursementRequestId && !r.IsDeleted);
                var request = ownerId.FirstOrDefault();
                // The Inventory Officer can open receipts of requests that have physical items to check.
                var inventoryAccess = request != null && roles.Contains(InventoryOfficerRole) &&
                    await _reimbursementRepository.CheckAsync(r => r.Id == request.Id && r.Items.Any(i => i.ItemType == ReimbursementItemType.Physical));
                if (request == null || (!IsPrivileged(roles) && request.PayeeUserId != userId && !inventoryAccess))
                {
                    return Fail<ReceiptFileDto>("Receipt not found.");
                }

                return new BaseResponse<ReceiptFileDto>
                {
                    Message = "Receipt found.",
                    Status = true,
                    Data = new ReceiptFileDto { FileName = receipt.FileName, ContentType = receipt.ContentType, Data = receipt.FileData }
                };
            }
            catch (Exception ex)
            {
                return Unexpected<ReceiptFileDto>(ex, "load the receipt");
            }
        }

        // =====================================================================
        // Notifications
        // =====================================================================

        public async Task<BaseResponse<ICollection<NotificationDto>>> GetNotificationsAsync(Guid userId)
        {
            try
            {
                var notifications = await _notificationRepository.GetForUserAsync(userId);
                var data = notifications.Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Message = n.Message,
                    Link = n.Link,
                    ReimbursementRequestId = n.ReimbursementRequestId,
                    IsRead = n.IsRead,
                    CreatedDate = n.CreatedDate
                }).ToList();

                return new BaseResponse<ICollection<NotificationDto>> { Message = "Notifications found.", Status = true, Data = data };
            }
            catch (Exception ex)
            {
                return Unexpected<ICollection<NotificationDto>>(ex, "load notifications");
            }
        }

        public async Task<BaseResponse<int>> GetUnreadNotificationCountAsync(Guid userId)
        {
            try
            {
                var count = await _notificationRepository.UnreadCountAsync(userId);
                return new BaseResponse<int> { Message = "Unread notifications counted.", Status = true, Data = count };
            }
            catch (Exception ex)
            {
                return Unexpected<int>(ex, "count notifications");
            }
        }

        public async Task<BaseResponse<bool>> MarkNotificationsReadAsync(Guid userId)
        {
            try
            {
                await _notificationRepository.MarkAllReadAsync(userId);
                return new BaseResponse<bool> { Message = "Notifications marked as read.", Status = true, Data = true };
            }
            catch (Exception ex)
            {
                return Unexpected<bool>(ex, "update notifications");
            }
        }

        // =====================================================================
        // Rules
        // =====================================================================

        private static bool IsAwaitingReview(ReimbursementStatus status) =>
            status is ReimbursementStatus.UnderReview or ReimbursementStatus.LegacyAccountsReview;

        /// <summary>
        /// Return and reject: the Accounts Officer or Accounts Admin while Under Review,
        /// the Accounts Admin while Pending Approval.
        /// </summary>
        private static string? CheckStageActor(ReimbursementRequest request, Guid userId, IReadOnlyCollection<string> roles, string verb)
        {
            if (request.PayeeUserId == userId)
            {
                return $"You cannot {verb} your own reimbursement.";
            }

            if (IsAwaitingReview(request.Status))
            {
                return roles.Contains(AccountsOfficerRole) || roles.Contains(AccountsAdminRole)
                    ? null
                    : $"While a request is under review, only an Accounts Officer or Accounts Admin can {verb} it.";
            }

            if (request.Status == ReimbursementStatus.PendingApproval)
            {
                return roles.Contains(AccountsAdminRole)
                    ? null
                    : $"While a request is pending approval, only an Accounts Admin can {verb} it.";
            }

            return $"A request that is {Describe(request.Status)} cannot be {(verb == "return" ? "returned" : "rejected")}.";
        }

        /// <summary>
        /// Null when every physical item has been verified. Otherwise a sentence saying what
        /// is still waiting or was rejected, used to block the officer and to require the
        /// Accounts Admin's override reason.
        /// </summary>
        private static string? InventoryBlockMessage(ReimbursementRequest request)
        {
            var physical = request.Items.Where(i => !i.IsDeleted && i.ItemType == ReimbursementItemType.Physical).ToList();
            var rejected = physical.Where(i => i.VerificationStatus == InventoryVerificationStatus.Rejected).ToList();
            var pending = physical.Where(i => i.VerificationStatus is InventoryVerificationStatus.Pending or InventoryVerificationStatus.NotRequired).ToList();

            if (rejected.Count > 0)
            {
                return "The Inventory Officer rejected " +
                    string.Join("; ", rejected.Select(i => $"\"{i.ItemName}\"" + (string.IsNullOrWhiteSpace(i.VerificationNotes) ? "" : $" ({i.VerificationNotes})"))) + ".";
            }

            if (pending.Count > 0)
            {
                return $"{pending.Count} physical item{(pending.Count == 1 ? " is" : "s are")} still waiting for the Inventory Officer to verify: " +
                    string.Join(", ", pending.Select(i => $"\"{i.ItemName}\"")) + ".";
            }

            return null;
        }

        private static bool IsPrivileged(IReadOnlyCollection<string> roles) =>
            roles.Contains(AccountsOfficerRole) || roles.Contains(AccountsAdminRole);

        private sealed record ClassificationResult(
            ReimbursementClassification Classification,
            Guid? DepartmentId,
            string DisplayName,
            string? Error);

        /// <summary>
        /// Validates the department choice on the server. A department must exist and be
        /// active. When editing, keeping the department already on the request is allowed
        /// even if it has since been deactivated; submitting is then blocked with a clear message.
        /// </summary>
        private async Task<ClassificationResult> ResolveClassificationAsync(
            ReimbursementClassification classification, Guid? departmentId, Guid? currentDepartmentId)
        {
            if (!Enum.IsDefined(classification))
            {
                return new ClassificationResult(classification, null, string.Empty, "Choose a department or Personal Use.");
            }

            if (classification == ReimbursementClassification.PersonalUse)
            {
                return new ClassificationResult(classification, null, PersonalUseLabel, null);
            }

            if (departmentId == null || departmentId == Guid.Empty)
            {
                return new ClassificationResult(classification, null, string.Empty, "Choose a department or Personal Use.");
            }

            var department = await _departmentRepository.GetByIdAsync(departmentId.Value);
            if (department == null)
            {
                return new ClassificationResult(classification, null, string.Empty, "The selected department does not exist.");
            }

            if (!department.IsActive && department.Id != currentDepartmentId)
            {
                return new ClassificationResult(classification, null, string.Empty, $"{department.Name} is no longer accepting new requests.");
            }

            return new ClassificationResult(classification, department.Id, department.Name, null);
        }

        private static string? ValidateFields(
            string? payeeName,
            string? ministerInCharge,
            IEnumerable<ReimbursementItemRequestModel>? items,
            out List<ReimbursementItem> builtItems)
        {
            builtItems = [];

            if (Validator.CheckString(payeeName))
            {
                return "Payee name is required.";
            }

            if (Validator.CheckString(ministerInCharge))
            {
                return "Minister in charge is required.";
            }

            var source = items?.ToList() ?? [];

            if (source.Count == 0)
            {
                return "Add at least one item to the reimbursement.";
            }

            if (source.Count > MaxItems)
            {
                return $"A reimbursement can contain at most {MaxItems} items.";
            }

            for (var index = 0; index < source.Count; index++)
            {
                var item = source[index];

                if (Validator.CheckString(item.ItemName))
                {
                    return $"Item {index + 1} needs a description.";
                }

                if (item.Subtotal < 0 || item.Gst < 0 || item.Pst < 0 || item.Hst < 0)
                {
                    return $"Item {index + 1} amounts cannot be negative.";
                }

                if (!Enum.IsDefined(item.ItemType))
                {
                    return $"Item {index + 1}: choose whether it is a physical item or a service.";
                }

                var total = Math.Round(item.Subtotal + item.Gst + item.Pst + item.Hst, 2, MidpointRounding.AwayFromZero);
                var serial = item.SerialNumber?.Trim();
                if (serial is { Length: > 100 })
                {
                    return $"Item {index + 1}: the serial number is too long.";
                }

                builtItems.Add(new ReimbursementItem
                {
                    ReferenceNumber = item.ReferenceNumber > 0 ? item.ReferenceNumber : index + 1,
                    ItemName = item.ItemName.Trim(),
                    Subtotal = item.Subtotal,
                    Gst = item.Gst,
                    Pst = item.Pst,
                    Hst = item.Hst,
                    Total = total,
                    // Serial numbers are optional: only for items that already have one.
                    RequiresSerialNumber = false,
                    SerialNumber = string.IsNullOrWhiteSpace(serial) ? null : serial,
                    ScreeningNote = null,
                    ItemType = item.ItemType,
                    VerificationStatus = InventoryVerificationStatus.NotRequired
                });
            }

            return null;
        }

        private static void ApplyTotals(ReimbursementRequest request)
        {
            request.Subtotal = request.Items.Sum(i => i.Subtotal);
            request.Gst = request.Items.Sum(i => i.Gst);
            request.Pst = request.Items.Sum(i => i.Pst);
            request.Hst = request.Items.Sum(i => i.Hst);
            request.TotalAmount = request.Items.Sum(i => i.Total);
        }

        private async Task<string> GenerateRequestNumberAsync()
        {
            while (true)
            {
                var candidate = $"RMB-{DateTime.UtcNow:yyyyMM}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";
                if (!await _reimbursementRepository.CheckAsync(r => r.RequestNumber == candidate))
                {
                    return candidate;
                }
            }
        }

        private static bool TryBuildReceipts(
            IEnumerable<ReimbursementReceiptUploadModel>? uploads,
            Guid userId,
            out List<ReimbursementReceipt> receipts,
            out string? error)
        {
            receipts = [];
            error = null;

            foreach (var upload in uploads ?? [])
            {
                if (Validator.CheckString(upload.FileName) ||
                    Validator.CheckString(upload.ContentType) ||
                    Validator.CheckString(upload.DataBase64))
                {
                    error = "Each receipt needs a filename, content type and file data.";
                    return false;
                }

                if (!AllowedReceiptTypes.Contains(upload.ContentType))
                {
                    error = "Receipts must be PDF, JPEG, PNG or WebP files.";
                    return false;
                }

                var raw = upload.DataBase64;
                var comma = raw.IndexOf(',');
                if (comma >= 0)
                {
                    raw = raw[(comma + 1)..];
                }

                byte[] bytes;
                try
                {
                    bytes = Convert.FromBase64String(raw);
                }
                catch
                {
                    error = $"Receipt \"{upload.FileName}\" does not contain valid file data.";
                    return false;
                }

                if (bytes.Length == 0)
                {
                    error = $"Receipt \"{upload.FileName}\" is empty.";
                    return false;
                }

                if (bytes.Length > MaxReceiptBytes)
                {
                    error = $"Receipt \"{upload.FileName}\" is larger than 5 MB.";
                    return false;
                }

                receipts.Add(new ReimbursementReceipt
                {
                    FileName = Path.GetFileName(upload.FileName).Trim(),
                    ContentType = upload.ContentType.Trim(),
                    FileSize = bytes.Length,
                    FileData = bytes,
                    UploadedBy = userId
                });
            }

            return true;
        }

        // =====================================================================
        // Saving, history and notifications
        // =====================================================================

        private async Task<string?> TrySaveAsync()
        {
            try
            {
                await _unitOfWork.SaveChangesAsync();
                return null;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Concurrent change detected on a reimbursement.");
                _pendingEmails.Clear();
                return ConcurrencyMessage;
            }
        }

        private async Task AddHistoryAsync(
            ReimbursementRequest request,
            Guid actorId,
            string action,
            ReimbursementStatus? fromStatus,
            ReimbursementStatus? toStatus,
            string? notes)
        {
            var names = await _userRepository.GetDisplayNamesAsync([actorId]);
            var history = new ReimbursementHistory
            {
                ReimbursementRequestId = request.Id,
                ActorUserId = actorId,
                ActorName = names.TryGetValue(actorId, out var name) ? name : null,
                Action = action,
                FromStatus = fromStatus,
                ToStatus = toStatus,
                Notes = notes,
                Timestamp = DateTime.UtcNow
            };

            request.History.Add(history);
            _reimbursementRepository.TrackAsAdded(history);
        }

        private async Task QueueNotifyPayeeAsync(ReimbursementRequest request, Guid actorId, string title, string message)
        {
            var payee = await _userRepository.GetByIdAsync(request.PayeeUserId);
            if (payee != null)
            {
                await QueueNotifyAsync([payee], actorId, title, message, request.Id);
            }
        }

        private async Task QueueNotifyReviewerAsync(ReimbursementRequest request, Guid actorId, string title, string message)
        {
            if (request.ReviewedBy is not Guid reviewerId)
            {
                return;
            }

            var reviewer = await _userRepository.GetByIdAsync(reviewerId);
            if (reviewer != null)
            {
                await QueueNotifyAsync([reviewer], actorId, title, message, request.Id);
            }
        }

        private async Task QueueNotifyAsync(IEnumerable<User> users, Guid actorId, string title, string message, Guid requestId)
        {
            foreach (var user in users.Where(u => u.Id != actorId).DistinctBy(u => u.Id))
            {
                await _notificationRepository.CreateAsync(new Notification
                {
                    UserId = user.Id,
                    Title = title,
                    Message = message,
                    Link = $"/reimbursements/{requestId}",
                    ReimbursementRequestId = requestId
                });

                if (!string.IsNullOrWhiteSpace(user.Email))
                {
                    _pendingEmails.Add((user.Email, title, message));
                }
            }
        }

        private async Task FlushEmailsAsync()
        {
            if (!_emailSender.IsConfigured)
            {
                _pendingEmails.Clear();
                return;
            }

            foreach (var (to, title, message) in _pendingEmails)
            {
                try
                {
                    await _emailSender.SendAsync(to, title, EmailTemplates.Notification(title, message, null));
                }
                catch (Exception ex)
                {
                    // Best effort: the in-app notification is already saved.
                    _logger.LogWarning(ex, "Notification email '{Title}' to {To} failed.", title, to);
                }
            }

            _pendingEmails.Clear();
        }

        // =====================================================================
        // Mapping
        // =====================================================================

        private static IEnumerable<Guid> ActorIds(ReimbursementRequest r)
        {
            if (r.ReviewedBy is Guid reviewed) yield return reviewed;
            if (r.ApprovedBy is Guid approved) yield return approved;
            if (r.PaidBy is Guid paid) yield return paid;
            if (r.RejectedBy is Guid rejected) yield return rejected;
        }

        private async Task<Dictionary<Guid, string?>> GetEmailsAsync(IEnumerable<Guid> userIds)
        {
            var result = new Dictionary<Guid, string?>();
            foreach (var id in userIds.Distinct())
            {
                var user = await _userRepository.GetByIdAsync(id);
                result[id] = user?.Email;
            }

            return result;
        }

        private async Task<BaseResponse<ReimbursementRequestDto>> BuildResponseAsync(ReimbursementRequest request, string message)
        {
            var payee = await _userRepository.GetByIdAsync(request.PayeeUserId);
            var names = await _userRepository.GetDisplayNamesAsync(ActorIds(request));
            return new BaseResponse<ReimbursementRequestDto>
            {
                Message = message,
                Status = true,
                Data = ToDto(request, payee?.Email, names, request.Receipts)
            };
        }

        private static ReimbursementRequestDto ToDto(
            ReimbursementRequest request,
            string? payeeEmail,
            IReadOnlyDictionary<Guid, string> names,
            IEnumerable<ReimbursementReceipt> receipts)
        {
            string? NameOf(Guid? id) => id is Guid value && names.TryGetValue(value, out var n) ? n : null;

            var receiptList = receipts.Where(r => !r.IsDeleted).OrderByDescending(r => r.CreatedDate).ToList();
            var isPersonal = request.Classification == ReimbursementClassification.PersonalUse;
            var departmentName = isPersonal
                ? PersonalUseLabel
                : request.DepartmentRecord?.Name ?? request.Department;

            return new ReimbursementRequestDto
            {
                Id = request.Id,
                RequestNumber = request.RequestNumber,
                PayeeUserId = request.PayeeUserId,
                PayeeEmail = payeeEmail,
                PayeeName = request.PayeeName,
                Department = departmentName,
                Classification = request.Classification,
                DepartmentId = request.DepartmentId,
                NeedsDepartmentLink = !isPersonal && request.DepartmentId == null,
                MinisterInCharge = request.MinisterInCharge,
                RequestDate = request.RequestDate,
                Remarks = request.Remarks,
                Subtotal = request.Subtotal,
                Gst = request.Gst,
                Pst = request.Pst,
                Hst = request.Hst,
                TotalAmount = request.TotalAmount,
                Status = request.Status,
                StatusName = request.Status.ToString(),
                ReceiptCount = receiptList.Count,
                SubmittedDate = request.SubmittedDate,
                MinisterApprovedDate = request.MinisterApprovedDate,
                ReviewedDate = request.ReviewedDate,
                ReviewedByName = NameOf(request.ReviewedBy),
                ReviewNotes = request.ReviewNotes,
                ApprovedDate = request.ApprovedDate,
                ApprovedByName = NameOf(request.ApprovedBy),
                ApprovedAmount = request.ApprovedAmount,
                ApprovalNotes = request.ApprovalNotes,
                OverBudgetAtApproval = request.OverBudgetAtApproval,
                OverBudgetJustification = request.OverBudgetJustification,
                InventoryOverride = request.InventoryOverride,
                InventoryOverrideReason = request.InventoryOverrideReason,
                PendingVerificationCount = request.Items.Count(i => !i.IsDeleted && i.ItemType == ReimbursementItemType.Physical &&
                    i.VerificationStatus is InventoryVerificationStatus.Pending or InventoryVerificationStatus.NotRequired),
                RejectedVerificationCount = request.Items.Count(i => !i.IsDeleted && i.ItemType == ReimbursementItemType.Physical &&
                    i.VerificationStatus == InventoryVerificationStatus.Rejected),
                PaymentDate = request.PaymentDate,
                PaymentConfirmedAt = request.PaymentConfirmedAt,
                PaidByName = NameOf(request.PaidBy),
                AmountPaid = request.AmountPaid,
                PaymentMethod = request.PaymentMethod,
                PaymentReference = request.PaymentReference,
                PreparedBy = request.PreparedBy,
                ChequeNumber = request.ChequeNumber,
                AuthorizedByName1 = request.AuthorizedByName1,
                AuthorizedByName2 = request.AuthorizedByName2,
                AuthorizationDate = request.AuthorizationDate,
                FinanceNotes = request.FinanceNotes,
                RejectedDate = request.RejectedDate,
                RejectedByName = NameOf(request.RejectedBy),
                RejectionReason = request.RejectionReason,
                InfoRequestNotes = request.InfoRequestNotes,
                CreatedDate = request.CreatedDate,
                Items = request.Items
                    .OrderBy(i => i.ReferenceNumber)
                    .Select(i => new ReimbursementItemDto
                    {
                        Id = i.Id,
                        ReferenceNumber = i.ReferenceNumber,
                        ItemName = i.ItemName,
                        Subtotal = i.Subtotal,
                        Gst = i.Gst,
                        Pst = i.Pst,
                        Hst = i.Hst,
                        Total = i.Total,
                        RequiresSerialNumber = i.RequiresSerialNumber,
                        SerialNumber = i.SerialNumber,
                        ScreeningNote = i.ScreeningNote,
                        ItemType = i.ItemType,
                        VerificationStatus = i.VerificationStatus,
                        VerifiedByName = i.VerifiedByName,
                        VerifiedDate = i.VerifiedDate,
                        VerificationNotes = i.VerificationNotes,
                        InventoryItemId = i.InventoryItemId
                    })
                    .ToList(),
                Receipts = receiptList
                    .Select(r => new ReimbursementReceiptDto
                    {
                        Id = r.Id,
                        FileName = r.FileName,
                        ContentType = r.ContentType,
                        FileSize = r.FileSize,
                        UploadedBy = r.UploadedBy,
                        CreatedDate = r.CreatedDate
                    })
                    .ToList(),
                History = request.History
                    .Where(h => !h.IsDeleted)
                    .OrderByDescending(h => h.Timestamp)
                    .Select(h => new ReimbursementHistoryDto
                    {
                        Id = h.Id,
                        Action = h.Action,
                        ActorUserId = h.ActorUserId,
                        ActorName = h.ActorName,
                        FromStatus = h.FromStatus,
                        ToStatus = h.ToStatus,
                        Notes = h.Notes,
                        Timestamp = h.Timestamp
                    })
                    .ToList()
            };
        }

        private static string Describe(ReimbursementStatus status) => status switch
        {
            ReimbursementStatus.Draft => "a draft",
            ReimbursementStatus.UnderReview => "under review",
            ReimbursementStatus.LegacyAccountsReview => "under review",
            ReimbursementStatus.PendingApproval => "pending approval",
            ReimbursementStatus.Approved => "approved and awaiting payment",
            ReimbursementStatus.Paid => "already paid",
            ReimbursementStatus.Rejected => "rejected",
            ReimbursementStatus.ReturnedForCorrection => "back with the member for correction",
            _ => status.ToString()
        };

        private static AuditLog NewAudit(Guid entityId, string action, Guid userId, string? details = null)
        {
            return new AuditLog
            {
                EntityName = nameof(ReimbursementRequest),
                EntityId = entityId,
                Action = action,
                UserId = userId,
                Details = details
            };
        }

        private static string FormatMoney(decimal amount)
        {
            return amount.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("en-CA"));
        }

        private static BaseResponse<T> Fail<T>(string message)
        {
            return new BaseResponse<T> { Message = message, Status = false, Data = default };
        }

        private BaseResponse<T> Unexpected<T>(Exception ex, string action)
        {
            _logger.LogError(ex, "Failed to {Action}.", action);
            return Fail<T>($"Something went wrong while trying to {action}. Please try again.");
        }
    }
}
