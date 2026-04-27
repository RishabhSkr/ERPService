namespace MyERP.Services.Inventory.Models
{
    public class StorageLocationType
    {
        public Guid Id { get; set; }
        
        public string TypeCode { get; set; } = string.Empty;
        public string TypeName { get; set; } = string.Empty;
        
        // Smart Validation Flags
        public bool AllowRawMaterials { get; set; } = false;
        public bool AllowProducts { get; set; } = false;
        
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation Property
        public ICollection<StorageLocation>? StorageLocations { get; set; }
    }
}
