using ChurchPortal.Core.Dtos;
using ChurchPortal.Core.Dtos.Request;
using ChurchPortal.Core.Entities;
using ChurchPortal.Repositories.Interfaces;
using ChurchPortal.Services.Interfaces;

namespace ChurchPortal.Services.Implementations
{
    public class InventoryItemService : IInventoryItemService
    {
        private readonly IInventoryItemRepository _inventoryItemRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IUnitOfWork _unitOfWork;

        public InventoryItemService(IInventoryItemRepository inventoryItemRepository, IAuditLogRepository auditLogRepository, IUnitOfWork unitOfWork)
        {
            _inventoryItemRepository = inventoryItemRepository;
            _auditLogRepository = auditLogRepository;
            _unitOfWork = unitOfWork;
        }

        private static InventoryItemDto ToDto(InventoryItem i) => new InventoryItemDto
        {
            Id = i.Id,
            ItemName = i.ItemName,
            Category = i.Category,
            Quantity = i.Quantity,
            Location = i.Location,
            Condition = i.Condition,
            PurchaseDate = i.PurchaseDate,
            Value = i.Value,
            Description = i.Description,
            Custodian = i.Custodian,
            SerialNumber = i.SerialNumber,
            LastVerifiedDate = i.LastVerifiedDate
        };

        public async Task<BaseResponse<InventoryItemDto>> CreateAsync(CreateInventoryItemRequestModel model, Guid userId)
        {
            try
            {
                if (Validator.CheckNull(model) || Validator.CheckString(model.ItemName))
                {
                    return new BaseResponse<InventoryItemDto> { Message = "Item name is required.", Status = false, Data = null };
                }
                if (Validator.CheckNegative(model.Quantity))
                {
                    return new BaseResponse<InventoryItemDto> { Message = "Quantity cannot be negative.", Status = false, Data = null };
                }

                var item = new InventoryItem
                {
                    ItemName = model.ItemName,
                    Category = model.Category,
                    Quantity = model.Quantity,
                    Location = model.Location,
                    Condition = model.Condition,
                    PurchaseDate = Validator.AsUtc(model.PurchaseDate),
                    Value = model.Value,
                    Description = model.Description,
                    Custodian = model.Custodian,
                    SerialNumber = model.SerialNumber,
                    LastVerifiedDate = Validator.AsUtc(model.LastVerifiedDate)
                };

                await _inventoryItemRepository.CreateAsync(item);
                await _auditLogRepository.CreateAsync(new AuditLog
                {
                    EntityName = nameof(InventoryItem),
                    EntityId = item.Id,
                    Action = "Create",
                    UserId = userId
                });
                await _unitOfWork.SaveChangesAsync();

                return new BaseResponse<InventoryItemDto> { Message = "Inventory item created successfully.", Status = true, Data = ToDto(item) };
            }
            catch (Exception ex)
            {
                return new BaseResponse<InventoryItemDto> { Message = ex.Message, Status = false, Data = null };
            }
        }

        public async Task<BaseResponse<ICollection<InventoryItemDto>>> GetAll()
        {
            try
            {
                var items = await _inventoryItemRepository.GetAllAsync(a => !a.IsDeleted);
                return new BaseResponse<ICollection<InventoryItemDto>> { Message = "Inventory items found", Status = true, Data = items.Select(ToDto).ToList() };
            }
            catch (Exception ex)
            {
                return new BaseResponse<ICollection<InventoryItemDto>> { Message = ex.Message, Status = false, Data = null };
            }
        }

        public async Task<BaseResponse<InventoryItemDto>> GetAsync(Guid id)
        {
            var item = await _inventoryItemRepository.GetByIdAsync(id);
            if (item == null)
            {
                return new BaseResponse<InventoryItemDto> { Message = "Inventory item not found.", Status = false, Data = null };
            }
            return new BaseResponse<InventoryItemDto> { Message = "Inventory item found", Status = true, Data = ToDto(item) };
        }

        public async Task<BaseResponse<InventoryItemDto>> UpdateAsync(UpdateInventoryItemModel model, Guid userId)
        {
            try
            {
                var item = await _inventoryItemRepository.GetByIdAsync(model.Id);
                if (item == null)
                {
                    return new BaseResponse<InventoryItemDto> { Message = "Inventory item not found.", Status = false, Data = null };
                }

                item.ItemName = model.ItemName;
                item.Category = model.Category;
                item.Quantity = model.Quantity;
                item.Location = model.Location;
                item.Condition = model.Condition;
                item.PurchaseDate = Validator.AsUtc(model.PurchaseDate);
                item.Value = model.Value;
                item.Description = model.Description;
                item.Custodian = model.Custodian;
                item.SerialNumber = model.SerialNumber;
                item.LastVerifiedDate = Validator.AsUtc(model.LastVerifiedDate);

                await _inventoryItemRepository.Update(item);
                await _auditLogRepository.CreateAsync(new AuditLog
                {
                    EntityName = nameof(InventoryItem),
                    EntityId = item.Id,
                    Action = "Update",
                    UserId = userId
                });
                await _unitOfWork.SaveChangesAsync();

                return new BaseResponse<InventoryItemDto> { Message = "Inventory item updated successfully.", Status = true, Data = ToDto(item) };
            }
            catch (Exception ex)
            {
                return new BaseResponse<InventoryItemDto> { Message = ex.Message, Status = false, Data = null };
            }
        }

        public async Task<BaseResponse<bool>> SoftDeleteAsync(Guid id, Guid userId)
        {
            try
            {
                var item = await _inventoryItemRepository.GetByIdAsync(id);
                if (item == null)
                {
                    return new BaseResponse<bool> { Message = "Inventory item not found.", Status = false, Data = false };
                }

                await _inventoryItemRepository.SoftDeleteAsync(item);
                await _auditLogRepository.CreateAsync(new AuditLog
                {
                    EntityName = nameof(InventoryItem),
                    EntityId = item.Id,
                    Action = "Delete",
                    UserId = userId
                });
                await _unitOfWork.SaveChangesAsync();

                return new BaseResponse<bool> { Message = "Inventory item deleted successfully.", Status = true, Data = true };
            }
            catch (Exception ex)
            {
                return new BaseResponse<bool> { Message = ex.Message, Status = false, Data = false };
            }
        }
    }
}
