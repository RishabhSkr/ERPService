namespace MyERP.Services.Inventory.DTOs.StockMovements
{
    public class RecordStockMovementDto
    {
        public string MovementType { get; set; } = string.Empty; // IN, OUT, RESERVE, RELEASE, ADJUST, SCRAP
        public string ItemType { get; set; } = string.Empty; // Product, RawMaterial
        public Guid ItemId { get; set; }
        public Guid WarehouseId { get; set; }
        public decimal Quantity { get; set; }
        public string? ReferenceType { get; set; } // SalesOrder, ProductionOrder, Purchase
        public Guid? ReferenceId { get; set; }
        public string? Notes { get; set; }
    }
}
