
namespace MyERP.Services.Inventory.Models
{
    public class StorageLocation
    {
        public Guid Id { get; set; }
        
        // Foreign Key to Warehouse
        public Guid WarehouseId { get; set; }
        public Warehouse? Warehouse { get; set; }
        
        // Foreign Key to StorageLocationType
        public Guid? LocationTypeId { get; set; }
        public StorageLocationType? LocationType { get; set; }
        
        // Flattened Hierarchy Fields
        public string? Zone { get; set; }      // e.g., Dry Goods, Refrigerated
        public string? Aisle { get; set; }     // e.g., A, B, C
        public string? Rack { get; set; }      // e.g., R01, R02
        public string? Level { get; set; }     // e.g., L1, L2
        public string? Bin { get; set; }       // e.g., B01, B02
        
        // Unique Scannable Code (e.g. Z1-A01-R02-L1-B01)
        public string LocationCode { get; set; } = string.Empty;
        
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public ICollection<ProductInventory>? ProductInventories { get; set; }
        public ICollection<RawMaterialInventory>? RawMaterialInventories { get; set; }
    }
}
