namespace MyERP.Services.Inventory.Models
{
    public class Product
    {
        public Guid Id { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string? Description { get; set; }
        
        // Foreign Keys
        public Guid CategoryId { get; set; }
        public Category? Category { get; set; }
        
        public Guid UnitId { get; set; }
        public Unit? Unit { get; set; }
        
        public Guid? DefaultStorageLocationId { get; set; }
        public StorageLocation? DefaultStorageLocation { get; set; }
        
        public decimal Price { get; set; }
        public decimal MinStockLevel { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public ICollection<ProductInventory>? ProductInventories { get; set; }
    }
}
