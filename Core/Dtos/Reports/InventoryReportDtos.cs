namespace ChurchPortal.Core.Dtos.Reports
{
    public class CategoryValuationDto
    {
        public string Category { get; set; } = string.Empty;
        public decimal TotalValue { get; set; }
        public int ItemCount { get; set; }
    }

    public class InventoryValuationDto
    {
        public decimal TotalValue { get; set; }
        public List<CategoryValuationDto> ByCategory { get; set; } = new();
    }

    public class ReplacementAlertDto
    {
        public Guid ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Condition { get; set; } = string.Empty;
        public DateTime PurchaseDate { get; set; }
        public double AgeInYears { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    public class MissingVerificationDto
    {
        public Guid ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public DateTime? LastVerifiedDate { get; set; }
        public double? MonthsSinceVerified { get; set; }
    }

    public class CustodianItemDto
    {
        public Guid ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public decimal Value { get; set; }
    }

    public class CustodianAccountabilityDto
    {
        public string Custodian { get; set; } = string.Empty;
        public int ItemCount { get; set; }
        public decimal TotalValue { get; set; }
        public List<CustodianItemDto> Items { get; set; } = new();
    }
}
