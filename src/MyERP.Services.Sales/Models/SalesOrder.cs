namespace MyERP.Services.Sales.Models
{
    public class SalesOrder
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;  // SO-2026-0001
        public Guid CustomerId { get; set; }
        public DateTime OrderDate { get; set; }
        public string OrderStatus { get; set; } = "Pending";  // Pending, Confirmed, InProduction, Shipped, Delivered, Cancelled
        public decimal TotalAmount { get; set; }
        public string? Notes { get; set; }
        public Guid? CreatedBy { get; set; }  // UserId from Identity
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public virtual Customer? Customer { get; set; }
        public virtual ICollection<SalesOrderItem>? Items { get; set; }
    }
}
