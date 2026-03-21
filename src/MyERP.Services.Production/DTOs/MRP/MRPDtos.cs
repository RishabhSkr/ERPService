namespace MyERP.Services.Production.DTOs.MRP
{
    public class RunMRPRequestDto
    {
        public List<string>? IncludeStatuses { get; set; }  // e.g. ["Create", "Released"]
        public Guid? ProductId { get; set; }                 // Filter by specific product
    }

    public class MRPResultDto
    {
        public DateTime RunDate { get; set; } = DateTime.UtcNow;
        public int TotalOrders { get; set; }
        public int TotalProducts { get; set; }
        public List<MaterialRequirementLineDto> MaterialRequirements { get; set; } = new();
        public List<PurchaseSuggestionDto> PurchaseSuggestions { get; set; } = new();
    }

    public class MaterialRequirementLineDto
    {
        public Guid RawMaterialId { get; set; }
        public string MaterialCode { get; set; } = string.Empty;
        public string MaterialName { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal GrossRequirement { get; set; }
        public decimal CurrentStock { get; set; }
        public decimal ReservedStock { get; set; }
        public decimal AvailableStock { get; set; }
        public decimal NetRequirement { get; set; }
        public bool NeedsPurchase { get; set; }
        public List<string> UsedInOrders { get; set; } = new();
    }

    public class PurchaseSuggestionDto
    {
        public Guid RawMaterialId { get; set; }
        public string MaterialCode { get; set; } = string.Empty;
        public string MaterialName { get; set; } = string.Empty;
        public decimal QuantityToOrder { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;  // High, Medium, Low
    }
}
