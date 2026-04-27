namespace MyERP.Services.Inventory.DTOs.Warehouse
{
    public class StorageLocationTypeDto
    {
        public Guid Id { get; set; }
        public string TypeCode { get; set; } = string.Empty;
        public string TypeName { get; set; } = string.Empty;
        public bool AllowRawMaterials { get; set; }
        public bool AllowProducts { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateStorageLocationTypeDto
    {
        public string TypeCode { get; set; } = string.Empty;
        public string TypeName { get; set; } = string.Empty;
        public bool AllowRawMaterials { get; set; }
        public bool AllowProducts { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateStorageLocationTypeDto
    {
        public string TypeName { get; set; } = string.Empty;
        public bool AllowRawMaterials { get; set; }
        public bool AllowProducts { get; set; }
        public bool IsActive { get; set; }
    }
}
