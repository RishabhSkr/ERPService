namespace MyERP.Services.Sales.DTOs
{
    /// <summary>
    /// DTO for product info fetched from Inventory Service
    /// </summary>
    public class ProductDto
    {
        public Guid Id { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal AvailableStock { get; set; }
        public bool IsActive { get; set; }
    }
}
