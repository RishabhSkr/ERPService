namespace MyERP.Services.Inventory.DTOs.StockMovements
{
    public class StockMovementResponseDto
    {
        public Guid Id { get; set; }
        public string MovementType { get; set; } = string.Empty;
        public string ItemType { get; set; } = string.Empty;
        public Guid ItemId { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal StockBefore { get; set; }
        public decimal StockAfter { get; set; }
        public string? ReferenceType { get; set; }
        public Guid? ReferenceId { get; set; }
        public string? Notes { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
