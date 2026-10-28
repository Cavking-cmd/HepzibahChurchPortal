namespace ChurchPortal.Core.Entities
{
    public class ReimbursementItem : BaseEntity
    {
        public Guid ReimbursementRequestId { get; set; }
        public ReimbursementRequest? ReimbursementRequest { get; set; }

        public int ReferenceNumber { get; set; }
        public required string ItemName { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Gst { get; set; }
        public decimal Pst { get; set; }
        public decimal Hst { get; set; }
        public decimal Total { get; set; }

        /// <summary>No longer enforced. Kept for older rows.</summary>
        public bool RequiresSerialNumber { get; set; }
        /// <summary>Optional. Only for items that already carry a manufacturer serial number.</summary>
        public string? SerialNumber { get; set; }
        public string? ScreeningNote { get; set; }

        public ReimbursementItemType ItemType { get; set; } = ReimbursementItemType.Service;

        // Inventory Officer check (physical items only).
        public InventoryVerificationStatus VerificationStatus { get; set; } = InventoryVerificationStatus.NotRequired;
        public Guid? VerifiedBy { get; set; }
        public string? VerifiedByName { get; set; }
        public DateTime? VerifiedDate { get; set; }
        /// <summary>Inventory Officer's note, or the reason when the item was rejected.</summary>
        public string? VerificationNotes { get; set; }

        /// <summary>The inventory record created when the item was verified.</summary>
        public Guid? InventoryItemId { get; set; }
        public InventoryItem? InventoryItem { get; set; }
    }
}
