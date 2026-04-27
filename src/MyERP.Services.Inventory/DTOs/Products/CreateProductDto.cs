namespace MyERP.Services.Inventory.DTOs.Products
{
    public class CreateProductDto
    {
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid CategoryId { get; set; }
        public Guid UnitId { get; set; }
        public decimal Price { get; set; }
        public decimal MinStockLevel { get; set; }
        public Guid? DefaultStorageLocationId { get; set; }
    }
}
