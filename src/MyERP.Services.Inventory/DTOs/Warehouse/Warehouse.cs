namespace MyERP.Services.Inventory.DTOs
{
    public class WarehouseDto
    {
        public Guid Id { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public string WarehouseCode { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? City { get; set; }
        public bool IsActive { get; set; }
        
        // Detailed Stock Breakdown
        public List<StorageLocationDetailDto>? StorageLocations { get; set; }
    }

    public class StorageLocationDetailDto
    {
        public Guid Id { get; set; }
        public string LocationCode { get; set; } = string.Empty;
        public string LocationTypeName { get; set; } = string.Empty;
        public string? Zone { get; set; }
        public string? Rack { get; set; }
        public bool IsActive { get; set; }
        public List<StoredItemDto>? StoredItems { get; set; }
    }

    public class StoredItemDto
    {
        public string ItemType { get; set; } = string.Empty; // "RawMaterial" or "Product"
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public decimal CurrentStock { get; set; }
        public decimal ReservedStock { get; set; }
        public decimal AvailableStock { get; set; }
    }

    public class CreateWarehouseDto
    {
        public string WarehouseName { get; set; } = string.Empty;
        public string WarehouseCode { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? City { get; set; }
    }

    public class UpdateWarehouseDto
    {
        public string WarehouseName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? City { get; set; }
        public bool IsActive { get; set; }
    }
}
