using System.Net;
using ChurchPortal.Core.Dtos;
using ChurchPortal.Core.Dtos.Request;
using ChurchPortal.Core.Entities;
using ChurchPortal.Repositories.Interfaces;
using ChurchPortal.Services.Interfaces;

namespace ChurchPortal.Services.Implementations
{
    public class ReimbursementService : IReimbursementService
    {
        private const string AccountsOfficerRole = "AccountsOfficer";
        private const string AccountsAdminRole = "AccountsAdmin";
        private const string PastorRole = "Pastor";
        private const int MaxItems = 10;
        private const long MaxReceiptBytes = 5 * 1024 * 1024;

        private static readonly HashSet<string> AllowedReceiptTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "application/pdf",
            "image/jpeg",
            "image/png",
            "image/webp"
        };

        private readonly IReimbursementRepository _reimbursementRepository;
        private readonly IUserRepository _userRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IEmailSender _emailSender;
        private readonly IUnitOfWork _unitOfWork;

        public ReimbursementService(
            IReimbursementRepository reimbursementRepository,
            IUserRepository userRepository,
            INotificationRepository notificationRepository,
            IAuditLogRepository auditLogRepository,
            IEmailSender emailSender,
            IUnitOfWork unitOfWork)
        {
            _reimbursementRepository = reimbursementRepository;
            _userRepository = userRepository;
            _notificationRepository = notificationRepository;
            _auditLogRepository = auditLogRepository;
            _emailSender = emailSender;
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResponse<ReimbursementRequestDto>> CreateAsync(CreateReimbursementRequestModel model, Guid userId)
        {
            try
            {
                var error = ValidateFields(model.PayeeName, model.Department, model.MinisterInCharge, model.Items, out var items);
                if (error != null)
                {
                    return Fail<ReimbursementRequestDto>(error);
                }

                if (!TryBuildReceipts(model.Receipts, userId, out var receipts, out var receiptError))
                {
                    return Fail<ReimbursementRequestDto>(receiptError!);
                }

                var request = new ReimbursementRequest
                {
                    RequestNumber = await GenerateRequestNumberAsync(),
                    PayeeUserId = userId,
                    PayeeName = model.PayeeName.Trim(),
                    Department = model.Department.Trim(),
                    MinisterInCharge = model.MinisterInCharge.Trim(),
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

                return await BuildResponseAsync(request, "Reimbursement draft created successfully.");
            }
            catch (Exception ex)
            {
                return Fail<ReimbursementRequestDto>(ex.Message);
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

                if (request.PayeeUserId != userId && !await IsAccountsAdminAsync(userId))
                {
                    return Fail<ReimbursementRequestDto>("You can only edit your own reimbursement.");
                }

                if (request.Status is not (ReimbursementStatus.Draft or ReimbursementStatus.InfoRequested))
                {
                    return Fail<ReimbursementRequestDto>("Only draft or information-requested reimbursements can be edited.");
                }

                var error = ValidateFields(model.PayeeName, model.Department, model.MinisterInCharge, model.Items, out var items);
                if (error != null)
                {
                    return Fail<ReimbursementRequestDto>(error);
                }

                if (!TryBuildReceipts(model.Receipts, userId, out var receipts, out var receiptError))
                {
                    return Fail<ReimbursementRequestDto>(receiptError!);
                }

                request.PayeeName = model.PayeeName.Trim();
                request.Department = model.Department.Trim();
                request.MinisterInCharge = model.MinisterInCharge.Trim();
                request.RequestDate = Validator.AsUtc(model.RequestDate);
                request.Remarks = model.Remarks?.Trim();

                request.Items.Clear();
                foreach (var item in items)
                {
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
                await AddHistoryAsync(request, userId, "Updated", request.Status, request.Status, "Draft updated.");

                await _auditLogRepository.CreateAsync(NewAudit(request.Id, "Update", userId));
                await _unitOfWork.SaveChangesAsync();

                return await BuildResponseAsync(request, "Reimbursement draft updated successfully.");
            }
            catch (Exception ex)
            {
                return Fail<ReimbursementRequestDto>(ex.Message);
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

                if (request.PayeeUserId != userId && !await IsAccountsAdminAsync(userId))
                {
                    return Fail<ReimbursementRequestDto>("You can only submit your own reimbursement.");
                }

                if (request.Status is not (ReimbursementStatus.Draft or ReimbursementStatus.InfoRequested))
                {
                    return Fail<ReimbursementRequestDto>("Only draft or information-requested reimbursements can be submitted.");
                }

                if (request.Items.Count == 0)
                {
                    return Fail<ReimbursementRequestDto>("Add at least one item before submitting.");
                }

                if (!request.Receipts.Any(r => !r.IsDeleted))
                {
                    return Fail<ReimbursementRequestDto>("Attach at least one invoice or receipt before submitting. A reimbursement is void without a receipt.");
                }

                var missingSerial = request.Items.FirstOrDefault(i => i.RequiresSerialNumber && string.IsNullOrWhiteSpace(i.SerialNumber));
                if (missingSerial != null)
                {
                    return Fail<ReimbursementRequestDto>($"Serial number is required for \"{missingSerial.ItemName}\" before this request can be submitted.");
                }

                var fromStatus = request.Status;
                request.Status = ReimbursementStatus.Pending;
                request.SubmittedDate = DateTime.UtcNow;
                request.InfoRequestNotes = null;
                request.RejectionReason = null;
                await AddHistoryAsync(request, userId, "Submitted", fromStatus, ReimbursementStatus.Pending, "Submitted for minister approval.");

                var approvers = await GetUsersByRolesAsync(PastorRole, AccountsAdminRole);
                await NotifyAsync(approvers, userId, $"Reimbursement {request.RequestNumber} awaiting approval", $"{request.PayeeName} submitted a reimbursement of {FormatMoney(request.TotalAmount)} for approval.", request.Id);

                await _auditLogRepository.CreateAsync(NewAudit(request.Id, "Submit", userId));
                await _unitOfWork.SaveChangesAsync();

                return await BuildResponseAsync(request, "Reimbursement submitted for minister approval.");
            }
            catch (Exception ex)
            {
                return Fail<ReimbursementRequestDto>(ex.Message);
            }
        }

        public async Task<BaseResponse<ReimbursementRequestDto>> MinisterApproveAsync(Guid id, ReimbursementDecisionModel model, Guid userId)
        {
            try
            {
                var request = await _reimbursementRepository.GetWithDetailsAsync(id);
                if (request == null)
                {
                    return Fail<ReimbursementRequestDto>("Reimbursement not found.");
                }

                if (request.Status != ReimbursementStatus.Pending)
                {
                    return Fail<ReimbursementRequestDto>("Only pending reimbursements can be approved by the minister.");
                }

                request.Status = ReimbursementStatus.AccountsReview;
                request.MinisterApprovedBy = userId;
                request.MinisterApprovedDate = DateTime.UtcNow;
                await AddHistoryAsync(request, userId, "MinisterApproved", ReimbursementStatus.Pending, ReimbursementStatus.AccountsReview, model.Notes ?? "Approved by minister in charge.");

                var recipients = await GetUsersByRolesAsync(AccountsOfficerRole, AccountsAdminRole);
                var payee = await _userRepository.GetByIdAsync(request.PayeeUserId);
                if (payee != null)
                {
                    recipients.Add(payee);
                }

                await NotifyAsync(recipients, userId, $"Reimbursement {request.RequestNumber} ready for accounts review", $"{request.PayeeName}'s reimbursement of {FormatMoney(request.TotalAmount)} was approved by the minister and is ready for reconciliation.", request.Id);

                await _auditLogRepository.CreateAsync(NewAudit(request.Id, "MinisterApprove", userId));
                await _unitOfWork.SaveChangesAsync();

                return await BuildResponseAsync(request, "Reimbursement approved by minister and sent to accounts.");
            }
            catch (Exception ex)
            {
                return Fail<ReimbursementRequestDto>(ex.Message);
            }
        }

        public async Task<BaseResponse<ReimbursementRequestDto>> RequestInfoAsync(Guid id, RequestReimbursementInfoModel model, Guid userId)
        {
            try
            {
                var request = await _reimbursementRepository.GetWithDetailsAsync(id);
                if (request == null)
                {
                    return Fail<ReimbursementRequestDto>("Reimbursement not found.");
                }

                if (Validator.CheckString(model.Notes))
                {
                    return Fail<ReimbursementRequestDto>("Describe the information that is required.");
                }

                if (request.Status is not (ReimbursementStatus.Pending or ReimbursementStatus.AccountsReview or ReimbursementStatus.Approved))
                {
                    return Fail<ReimbursementRequestDto>("Information can only be requested for pending, in-review or approved reimbursements.");
                }

                var fromStatus = request.Status;
                request.Status = ReimbursementStatus.InfoRequested;
                request.InfoRequestNotes = model.Notes.Trim();
                await AddHistoryAsync(request, userId, "InfoRequested", fromStatus, ReimbursementStatus.InfoRequested, model.Notes.Trim());

                var payee = await _userRepository.GetByIdAsync(request.PayeeUserId);
                if (payee != null)
                {
                    await NotifyAsync([payee], userId, $"More information needed for {request.RequestNumber}", model.Notes.Trim(), request.Id);
                }

                await _auditLogRepository.CreateAsync(NewAudit(request.Id, "RequestInfo", userId));
                await _unitOfWork.SaveChangesAsync();

                return await BuildResponseAsync(request, "Information requested from the requester.");
            }
            catch (Exception ex)
            {
                return Fail<ReimbursementRequestDto>(ex.Message);
            }
        }

        public async Task<BaseResponse<ReimbursementRequestDto>> RejectAsync(Guid id, RejectReimbursementModel model, Guid userId)
        {
            try
            {
                var request = await _reimbursementRepository.GetWithDetailsAsync(id);
                if (request == null)
                {
                    return Fail<ReimbursementRequestDto>("Reimbursement not found.");
                }

                if (Validator.CheckString(model.Reason))
                {
                    return Fail<ReimbursementRequestDto>("A rejection reason is required.");
                }

                if (request.Status is ReimbursementStatus.Paid or ReimbursementStatus.Rejected or ReimbursementStatus.Draft)
                {
                    return Fail<ReimbursementRequestDto>("This reimbursement cannot be rejected in its current state.");
                }

                var fromStatus = request.Status;
                request.Status = ReimbursementStatus.Rejected;
                request.RejectionReason = model.Reason.Trim();
                await AddHistoryAsync(request, userId, "Rejected", fromStatus, ReimbursementStatus.Rejected, model.Reason.Trim());

                var payee = await _userRepository.GetByIdAsync(request.PayeeUserId);
                if (payee != null)
                {
                    await NotifyAsync([payee], userId, $"Reimbursement {request.RequestNumber} rejected", model.Reason.Trim(), request.Id);
                }

                await _auditLogRepository.CreateAsync(NewAudit(request.Id, "Reject", userId));
                await _unitOfWork.SaveChangesAsync();

                return await BuildResponseAsync(request, "Reimbursement rejected.");
            }
            catch (Exception ex)
            {
                return Fail<ReimbursementRequestDto>(ex.Message);
            }
        }

        public async Task<BaseResponse<ReimbursementRequestDto>> ApproveAsync(Guid id, ReimbursementDecisionModel model, Guid userId)
        {
            try
            {
                var request = await _reimbursementRepository.GetWithDetailsAsync(id);
                if (request == null)
                {
                    return Fail<ReimbursementRequestDto>("Reimbursement not found.");
                }

                if (request.Status != ReimbursementStatus.AccountsReview)
                {
                    return Fail<ReimbursementRequestDto>("Only reimbursements in accounts review can be approved.");
                }

                request.Status = ReimbursementStatus.Approved;
                request.ApprovedBy = userId;
                request.ReviewedBy = request.ReviewedBy ?? userId;
                request.ReviewedDate = request.ReviewedDate ?? DateTime.UtcNow;
                request.ApprovedDate = DateTime.UtcNow;
                await AddHistoryAsync(request, userId, "Approved", ReimbursementStatus.AccountsReview, ReimbursementStatus.Approved, model.Notes ?? "Approved for payment.");

                var recipients = await GetUsersByRolesAsync(AccountsOfficerRole);
                var payee = await _userRepository.GetByIdAsync(request.PayeeUserId);
                if (payee != null)
                {
                    recipients.Add(payee);
                }

                await NotifyAsync(recipients, userId, $"Reimbursement {request.RequestNumber} approved for payment", $"{request.PayeeName}'s reimbursement of {FormatMoney(request.TotalAmount)} was approved and is ready for payment.", request.Id);

                await _auditLogRepository.CreateAsync(NewAudit(request.Id, "Approve", userId));
                await _unitOfWork.SaveChangesAsync();

                return await BuildResponseAsync(request, "Reimbursement approved for payment.");
            }
            catch (Exception ex)
            {
                return Fail<ReimbursementRequestDto>(ex.Message);
            }
        }

        public async Task<BaseResponse<ReimbursementRequestDto>> RecordPaymentAsync(Guid id, RecordReimbursementPaymentModel model, Guid userId)
        {
            try
            {
                var request = await _reimbursementRepository.GetWithDetailsAsync(id);
                if (request == null)
                {
                    return Fail<ReimbursementRequestDto>("Reimbursement not found.");
                }

                if (request.Status != ReimbursementStatus.Approved)
                {
                    return Fail<ReimbursementRequestDto>("Only approved reimbursements can be paid.");
                }

                if (model.AmountPaid <= 0)
                {
                    return Fail<ReimbursementRequestDto>("The paid amount must be greater than zero.");
                }

                if (Validator.CheckString(model.PaymentMethod))
                {
                    return Fail<ReimbursementRequestDto>("Payment method is required.");
                }

                if (model.AmountPaid != request.TotalAmount && string.IsNullOrWhiteSpace(model.FinanceNotes))
                {
                    return Fail<ReimbursementRequestDto>("Add a finance note explaining the difference between the approved total and the amount paid.");
                }

                request.Status = ReimbursementStatus.Paid;
                request.PaidBy = userId;
                request.PaymentDate = Validator.AsUtc(model.PaymentDate);
                request.AmountPaid = model.AmountPaid;
                request.PaymentMethod = model.PaymentMethod.Trim();
                request.PaymentReference = model.PaymentReference?.Trim();
                request.ChequeNumber = model.ChequeNumber?.Trim();
                request.AuthorizedByName1 = model.AuthorizedByName1?.Trim();
                request.AuthorizedByName2 = model.AuthorizedByName2?.Trim();
                request.AuthorizationDate = Validator.AsUtc(model.AuthorizationDate);
                request.FinanceNotes = model.FinanceNotes?.Trim();
                await AddHistoryAsync(request, userId, "Paid", ReimbursementStatus.Approved, ReimbursementStatus.Paid, $"Paid {FormatMoney(model.AmountPaid)} via {request.PaymentMethod}.");

                var recipients = await GetUsersByRolesAsync(AccountsAdminRole);
                var payee = await _userRepository.GetByIdAsync(request.PayeeUserId);
                if (payee != null)
                {
                    recipients.Add(payee);
                }

                await NotifyAsync(recipients, userId, $"Reimbursement {request.RequestNumber} paid", $"{request.PayeeName}'s reimbursement of {FormatMoney(model.AmountPaid)} was paid via {request.PaymentMethod}.", request.Id);

                await _auditLogRepository.CreateAsync(NewAudit(request.Id, "Pay", userId));
                await _unitOfWork.SaveChangesAsync();

                return await BuildResponseAsync(request, "Payment recorded successfully.");
            }
            catch (Exception ex)
            {
                return Fail<ReimbursementRequestDto>(ex.Message);
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

                if (request.PayeeUserId != userId && !await IsAccountsAdminAsync(userId))
                {
                    return Fail<bool>("You can only delete your own draft.");
                }

                if (request.Status != ReimbursementStatus.Draft)
                {
                    return Fail<bool>("Only drafts can be deleted.");
                }

                await _auditLogRepository.CreateAsync(NewAudit(request.Id, "Delete", userId));
                await _reimbursementRepository.SoftDeleteAsync(request);

                return new BaseResponse<bool> { Message = "Draft deleted.", Status = true, Data = true };
            }
            catch (Exception ex)
            {
                return Fail<bool>(ex.Message);
            }
        }

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
                    return Fail<ReimbursementRequestDto>("You do not have access to this reimbursement.");
                }

                return await BuildResponseAsync(request, "Reimbursement found.");
            }
            catch (Exception ex)
            {
                return Fail<ReimbursementRequestDto>(ex.Message);
            }
        }

        public async Task<BaseResponse<ICollection<ReimbursementRequestDto>>> GetAllAsync(Guid userId, IReadOnlyCollection<string> roles)
        {
            try
            {
                var requests = IsPrivileged(roles)
                    ? await _reimbursementRepository.GetAllWithDetailsAsync(_ => true)
                    : await _reimbursementRepository.GetAllWithDetailsAsync(r => r.PayeeUserId == userId);

                var receiptCounts = await _reimbursementRepository.GetReceiptCountsAsync(requests.Select(r => r.Id).ToList());
                var emails = new Dictionary<Guid, string?>();

                foreach (var payeeId in requests.Select(r => r.PayeeUserId).Distinct())
                {
                    var payee = await _userRepository.GetByIdAsync(payeeId);
                    emails[payeeId] = payee?.Email;
                }

                var data = requests.Select(request =>
                {
                    var dto = ToDto(request, emails.TryGetValue(request.PayeeUserId, out var email) ? email : null);
                    dto.ReceiptCount = receiptCounts.TryGetValue(request.Id, out var count) ? count : 0;
                    return dto;
                }).ToList();

                return new BaseResponse<ICollection<ReimbursementRequestDto>> { Message = "Reimbursements found.", Status = true, Data = data };
            }
            catch (Exception ex)
            {
                return Fail<ICollection<ReimbursementRequestDto>>(ex.Message);
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

                var request = await _reimbursementRepository.GetWithDetailsAsync(receipt.ReimbursementRequestId);
                if (request == null)
                {
                    return Fail<ReceiptFileDto>("Reimbursement not found.");
                }

                if (!IsPrivileged(roles) && request.PayeeUserId != userId)
                {
                    return Fail<ReceiptFileDto>("You do not have access to this receipt.");
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
                return Fail<ReceiptFileDto>(ex.Message);
            }
        }

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
                return Fail<ICollection<NotificationDto>>(ex.Message);
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
                return Fail<int>(ex.Message);
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
                return Fail<bool>(ex.Message);
            }
        }

        private static string? ValidateFields(
            string? payeeName,
            string? department,
            string? ministerInCharge,
            IEnumerable<ReimbursementItemRequestModel>? items,
            out List<ReimbursementItem> builtItems)
        {
            builtItems = [];

            if (Validator.CheckString(payeeName))
            {
                return "Payee name is required.";
            }

            if (Validator.CheckString(department))
            {
                return "Department is required.";
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

                var total = Math.Round(item.Subtotal + item.Gst + item.Pst + item.Hst, 2, MidpointRounding.AwayFromZero);
                var serial = item.SerialNumber?.Trim();
                var requiresSerial = SerialNumberScreener.RequiresSerialNumber(item.ItemName);

                builtItems.Add(new ReimbursementItem
                {
                    ReferenceNumber = item.ReferenceNumber > 0 ? item.ReferenceNumber : index + 1,
                    ItemName = item.ItemName.Trim(),
                    Subtotal = item.Subtotal,
                    Gst = item.Gst,
                    Pst = item.Pst,
                    Hst = item.Hst,
                    Total = total,
                    RequiresSerialNumber = requiresSerial,
                    SerialNumber = string.IsNullOrWhiteSpace(serial) ? null : serial,
                    ScreeningNote = requiresSerial ? "Serialised equipment detected; a serial number is required before submission." : null
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

        private async Task<BaseResponse<ReimbursementRequestDto>> BuildResponseAsync(ReimbursementRequest request, string message)
        {
            var payee = await _userRepository.GetByIdAsync(request.PayeeUserId);
            return new BaseResponse<ReimbursementRequestDto>
            {
                Message = message,
                Status = true,
                Data = ToDto(request, payee?.Email)
            };
        }

        private static ReimbursementRequestDto ToDto(ReimbursementRequest request, string? payeeEmail)
        {
            return new ReimbursementRequestDto
            {
                Id = request.Id,
                RequestNumber = request.RequestNumber,
                PayeeUserId = request.PayeeUserId,
                PayeeEmail = payeeEmail,
                PayeeName = request.PayeeName,
                Department = request.Department,
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
                ReceiptCount = request.Receipts.Count(r => !r.IsDeleted),
                SubmittedDate = request.SubmittedDate,
                MinisterApprovedDate = request.MinisterApprovedDate,
                ReviewedDate = request.ReviewedDate,
                ApprovedDate = request.ApprovedDate,
                PaymentDate = request.PaymentDate,
                AmountPaid = request.AmountPaid,
                PaymentMethod = request.PaymentMethod,
                PaymentReference = request.PaymentReference,
                ChequeNumber = request.ChequeNumber,
                AuthorizedByName1 = request.AuthorizedByName1,
                AuthorizedByName2 = request.AuthorizedByName2,
                AuthorizationDate = request.AuthorizationDate,
                FinanceNotes = request.FinanceNotes,
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
                        ScreeningNote = i.ScreeningNote
                    })
                    .ToList(),
                Receipts = request.Receipts
                    .Where(r => !r.IsDeleted)
                    .OrderByDescending(r => r.CreatedDate)
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

        private async Task AddHistoryAsync(
            ReimbursementRequest request,
            Guid actorId,
            string action,
            ReimbursementStatus? fromStatus,
            ReimbursementStatus? toStatus,
            string? notes)
        {
            var history = BuildHistory(request.Id, actorId, await GetActorNameAsync(actorId), action, fromStatus, toStatus, notes);
            request.History.Add(history);
            _reimbursementRepository.TrackAsAdded(history);
        }

        private static ReimbursementHistory BuildHistory(
            Guid requestId,
            Guid actorId,
            string? actorName,
            string action,
            ReimbursementStatus? fromStatus,
            ReimbursementStatus? toStatus,
            string? notes)
        {
            return new ReimbursementHistory
            {
                ReimbursementRequestId = requestId,
                ActorUserId = actorId,
                ActorName = actorName,
                Action = action,
                FromStatus = fromStatus,
                ToStatus = toStatus,
                Notes = notes,
                Timestamp = DateTime.UtcNow
            };
        }

        private async Task<string?> GetActorNameAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            return user?.DisplayName ?? user?.Email;
        }

        private async Task<bool> IsAccountsAdminAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            return user?.UserRoles.Any(ur => ur.Role?.Name == AccountsAdminRole) == true;
        }

        private async Task<List<User>> GetUsersByRolesAsync(params string[] roleNames)
        {
            var users = new List<User>();

            foreach (var roleName in roleNames)
            {
                users.AddRange(await _userRepository.GetUsersByRoleAsync(roleName));
            }

            return users.DistinctBy(u => u.Id).ToList();
        }

        private async Task NotifyAsync(IEnumerable<User> users, Guid actorId, string title, string message, Guid requestId)
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

                try
                {
                    await _emailSender.SendAsync(
                        user.Email,
                        title,
                        $"<p>{WebUtility.HtmlEncode(message)}</p><p>Sign in to the church portal to review it.</p>");
                }
                catch
                {
                    // Email delivery is best-effort; the in-app notification is still recorded.
                }
            }
        }

        private static bool IsPrivileged(IReadOnlyCollection<string> roles)
        {
            return roles.Contains(AccountsOfficerRole) || roles.Contains(AccountsAdminRole) || roles.Contains(PastorRole);
        }

        private static AuditLog NewAudit(Guid entityId, string action, Guid userId)
        {
            return new AuditLog
            {
                EntityName = nameof(ReimbursementRequest),
                EntityId = entityId,
                Action = action,
                UserId = userId
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
    }
}
