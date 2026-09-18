using ChurchPortal.Core.Entities;

namespace ChurchPortal.Core.Dtos.Request
{
    public class InventoryItemDto
    {
        public Guid Id { get; set; }
        public required string ItemName { get; set; }
        public required string Category { get; set; }
        public int Quantity { get; set; }
        public required string Location { get; set; }
        public ItemCondition Condition { get; set; }
        public DateTime PurchaseDate { get; set; }
        public decimal Value { get; set; }
        public required string Description { get; set; }
        public DateTime? LastVerifiedDate { get; set; }
        public string? SerialNumber { get; set; }
        public required string Custodian { get; set; }
    }

    public class CreateInventoryItemRequestModel
    {
        public required string ItemName { get; set; }
        public required string Category { get; set; }
        public int Quantity { get; set; }
        public required string Location { get; set; }
        public ItemCondition Condition { get; set; }
        public DateTime PurchaseDate { get; set; }
        public decimal Value { get; set; }
        public required string Description { get; set; }
        public DateTime? LastVerifiedDate { get; set; }
        public string? SerialNumber { get; set; }
        public required string Custodian { get; set; }
    }

    public class UpdateInventoryItemModel
    {
        public required Guid Id { get; set; }
        public required string ItemName { get; set; }
        public required string Category { get; set; }
        public int Quantity { get; set; }
        public required string Location { get; set; }
        public ItemCondition Condition { get; set; }
        public DateTime PurchaseDate { get; set; }
        public decimal Value { get; set; }
        public required string Description { get; set; }
        public required string Custodian { get; set; }
        public DateTime? LastVerifiedDate { get; set; }
        public string? SerialNumber { get; set; }
    }
}
