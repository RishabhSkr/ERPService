using MyERP.Services.Inventory.Models;

namespace MyERP.Services.Inventory.Models
{
    public class Warehouse
    {
        public Guid Id { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public string WarehouseCode { get; set; } = string.Empty;
        public WarehouseType Type { get; set; } = WarehouseType.Physical;
        public string? Address { get; set; }
        public string? City { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public ICollection<ProductInventory>? ProductInventories { get; set; }
        public ICollection<RawMaterialInventory>? RawMaterialInventories { get; set; }
        public ICollection<StorageLocation>? StorageLocations { get; set; }
    }
}
