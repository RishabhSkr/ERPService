namespace MyERP.Services.Inventory.DTOs.Products
{
    public class ProductResponseDto
    {
        public Guid Id { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public CategoryInfo? Category { get; set; }
        public UnitInfo? Unit { get; set; }
        public decimal Price { get; set; }
        public decimal MinStockLevel { get; set; }
        public decimal TotalStock { get; set; }
        public decimal TotalReserved { get; set; }
        public decimal TotalAvailable { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Warehouse-wise stock
        public List<WarehouseStockInfo>? WarehouseStock { get; set; }
    }

    public class CategoryInfo
    {
        public Guid Id { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryCode { get; set; } = string.Empty;
    }

    public class UnitInfo
    {
        public Guid Id { get; set; }
        public string UnitName { get; set; } = string.Empty;
        public string UnitCode { get; set; } = string.Empty;
    }

    public class WarehouseStockInfo
    {
        public Guid WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public decimal CurrentStock { get; set; }
        public decimal ReservedStock { get; set; }
        public decimal AvailableStock { get; set; }
    }
}
