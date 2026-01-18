namespace MyERP.SalesServiceTutorial.Models;
public class SalesOrder
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;  // SO-2026-0001
    
    // Foreign Key - Which customer placed this order?
    public int CustomerId { get; set; }
    
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public decimal TotalAmount { get; set; }
    public string OrderStatus { get; set; } = "Pending";  // Pending, Confirmed, Cancelled
    public string PaymentStatus { get; set; } = "Unpaid"; // Unpaid, Paid, PartiallyPaid
    public string? Notes { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation property - Which customer placed this order?
    public Customer Customer { get; set; } = null!; // Many to One

    // Navigation property - What items are in this order? // One to Many
    public ICollection<SalesOrderItem> Items { get; set; } = new List<SalesOrderItem>(); // One to Many
}