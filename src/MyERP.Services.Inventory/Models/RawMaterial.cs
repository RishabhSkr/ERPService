namespace MyERP.Services.Inventory.Models
{
    public class RawMaterial
    {
        public Guid Id { get; set; }
        public string MaterialCode { get; set; } = string.Empty;
        public string MaterialName { get; set; } = string.Empty;
        public string? Description { get; set; }
        
        // Foreign Keys
        public Guid CategoryId { get; set; }
        public Category? Category { get; set; }
        
        public Guid UnitId { get; set; }
        public Unit? Unit { get; set; }
        
        public decimal Cost { get; set; }
        public decimal MinStockLevel { get; set; } = 0;
        public string? Supplier { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public ICollection<RawMaterialInventory>? RawMaterialInventories { get; set; }
    }
}
