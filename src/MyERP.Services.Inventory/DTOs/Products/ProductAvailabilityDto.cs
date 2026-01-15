namespace MyERP.Services.Inventory.DTOs.Products
{
    public class ProductAvailabilityDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal RequestedQuantity { get; set; }
        public decimal AvailableStock { get; set; }
        public bool IsAvailable { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
