namespace MyERP.SalesServiceTutorial.Models;
public class SalesOrderItem
{
    public int Id { get; set; }
    
    // Foreign Key - Which order does this item belong to?
    public int SalesOrderId { get; set; }
    
    // Reference to Inventory Service (cross-service reference)
    public int ProductId { get; set; }
    
    // Denormalized data (stored locally for display)
    public string ProductName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }  // Quantity * UnitPrice
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation Property
    public SalesOrder SalesOrder { get; set; } = null!;
}