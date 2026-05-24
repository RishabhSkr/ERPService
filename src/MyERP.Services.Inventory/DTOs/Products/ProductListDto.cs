namespace MyERP.Services.Inventory.DTOs.Products
{
    public class ProductListDto
    {
        public Guid Id { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal CurrentStock { get; set; }
        public decimal ReservedStock { get; set; }
        public decimal AvailableStock { get; set; }
        public string UnitName { get; set; } = string.Empty;
        public decimal MinStockLevel { get; set; }
        public Guid? DefaultStorageLocationId { get; set; }
        public string? DefaultStorageLocationCode { get; set; }
        public bool IsActive { get; set; }
        public List<LocationStockDto> LocationStocks { get; set; } = new();
    }
}
