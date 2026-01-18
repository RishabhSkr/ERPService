namespace MyERP.Services.Sales.Models
{
    public class SalesOrderItem
    {
        public Guid Id { get; set; }
        public Guid SalesOrderId { get; set; }
        public Guid ProductId { get; set; }  // Reference to Inventory.Products
        public string ProductCode { get; set; } = string.Empty;  // Denormalized
        public string ProductName { get; set; } = string.Empty;  // Denormalized
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }  // Quantity * UnitPrice
        public DateTime CreatedAt { get; set; }

        // Navigation
        public virtual SalesOrder? SalesOrder { get; set; }
    }
}
