namespace MyERP.Services.Inventory.DTOs.Warehouse
{
    public class StorageLocationDto
    {
        public Guid Id { get; set; }
        public Guid WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public string? Zone { get; set; }
        public string? Aisle { get; set; }
        public string? Rack { get; set; }
        public string? Level { get; set; }
        public string? Bin { get; set; }
        public string LocationCode { get; set; } = string.Empty;
        public Guid? LocationTypeId { get; set; }
        public string LocationTypeName { get; set; } = string.Empty;
        public bool AllowRawMaterials { get; set; }
        public bool AllowProducts { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateStorageLocationDto
    {
        public Guid WarehouseId { get; set; }
        public string? Zone { get; set; }
        public string? Aisle { get; set; }
        public string? Rack { get; set; }
        public string? Level { get; set; }
        public string? Bin { get; set; }
        public Guid? LocationTypeId { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateStorageLocationDto
    {
        public string? Zone { get; set; }
        public string? Aisle { get; set; }
        public string? Rack { get; set; }
        public string? Level { get; set; }
        public string? Bin { get; set; }
        public Guid? LocationTypeId { get; set; }
        public bool IsActive { get; set; }
        public Guid? WarehouseId { get; set; }
    }
}
