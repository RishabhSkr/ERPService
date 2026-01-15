namespace MyERP.Services.Inventory.Models
{
    public class ProductInventory
    {
        public Guid Id { get; set; }
        
        // Foreign Keys
        public Guid ProductId { get; set; }
        public Product? Product { get; set; }
        
        public Guid WarehouseId { get; set; }
        public Warehouse? Warehouse { get; set; }
        
        public string? LocationCode { get; set; } // Rack-A-15
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
