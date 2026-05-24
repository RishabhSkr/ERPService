namespace MyERP.Services.Inventory.DTOs.Products
{
    public class UpdateProductDto
    {
        public string? ProductName { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public decimal? MinStockLevel { get; set; }
        public bool? IsActive { get; set; }
        public Guid? DefaultStorageLocationId { get; set; }
    }
}
