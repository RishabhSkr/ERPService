namespace MyERP.Services.Inventory.Models
{
    public class RawMaterialInventory
    {
        public Guid Id { get; set; }
        
        // Foreign Keys
        public Guid RawMaterialId { get; set; }
        public RawMaterial? RawMaterial { get; set; }
        
        public Guid WarehouseId { get; set; }
        public Warehouse? Warehouse { get; set; }
        
        public string? LocationCode { get; set; } // Bin-B-20
        public decimal CurrentStock { get; set; } = 0;
        public decimal ReservedStock { get; set; } = 0;
        public string? BatchNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Computed property
        public decimal AvailableStock => CurrentStock - ReservedStock;
    }
}
