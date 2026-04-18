namespace MyERP.Services.Inventory.Models
{
    public class StockMovement
    {
        public Guid Id { get; set; }
        
        // Item identification
        public string ItemType { get; set; } = string.Empty; // "Product" or "RawMaterial"
        public Guid ItemId { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        
        // Warehouse
        public Guid WarehouseId { get; set; }
        public Warehouse? Warehouse { get; set; }
        
        // Movement details
        public string MovementType { get; set; } = string.Empty; // IN, OUT, RESERVE, RELEASE, ADJUST, SCRAP
        public decimal Quantity { get; set; }
        public decimal StockBefore { get; set; }
        public decimal StockAfter { get; set; }
        
        // Reference
        public string? ReferenceType { get; set; } // SalesOrder, ProductionOrder, Purchase
        public Guid? ReferenceId { get; set; }
        public Guid? WorkOrderId { get; set; }    // null = PO-level, has value = WO-level
        public string? Notes { get; set; }
        
        // Audit
        public Guid? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
