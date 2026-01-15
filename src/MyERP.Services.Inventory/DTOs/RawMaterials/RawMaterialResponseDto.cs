using MyERP.Services.Inventory.DTOs.Products;

namespace MyERP.Services.Inventory.DTOs.RawMaterials
{
    public class RawMaterialResponseDto
    {
        public Guid Id { get; set; }
        public string MaterialCode { get; set; } = string.Empty;
        public string MaterialName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public CategoryInfo? Category { get; set; }
        public UnitInfo? Unit { get; set; }
        public decimal Cost { get; set; }
        public decimal MinStockLevel { get; set; }
        public string? Supplier { get; set; }
        public decimal TotalStock { get; set; }
        public decimal TotalReserved { get; set; }
        public decimal TotalAvailable { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Warehouse-wise stock
        public List<WarehouseStockInfo>? WarehouseStock { get; set; }
    }
}
