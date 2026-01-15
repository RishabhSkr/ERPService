namespace MyERP.Services.Inventory.Models
{
    public class Unit
    {
        public Guid Id { get; set; }
        public string UnitName { get; set; } = string.Empty;
        public string UnitCode { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public ICollection<Product>? Products { get; set; }
        public ICollection<RawMaterial>? RawMaterials { get; set; }
    }
}
