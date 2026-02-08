namespace MyERP.Services.Sales.Events
{
    /// <summary>
    /// Event published when a new sales order is created.
    /// Consumed by Production Service to create PendingRequest.
    /// </summary>
    public class SalesOrderCreatedEvent
    {
        public Guid EventId { get; set; } = Guid.NewGuid();
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
        public string EventType { get; set; } = "SalesOrderCreated";
        
        // Payload
        public Guid SalesOrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public List<SalesOrderItemEvent> Items { get; set; } = new();
    }

    public class SalesOrderItemEvent
    {
        public Guid ProductId { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    /// <summary>
    /// Event published when a sales order is cancelled.
    /// Consumed by Production Service to cancel related pending requests.
    /// </summary>
    public class SalesOrderCancelledEvent
    {
        public Guid EventId { get; set; } = Guid.NewGuid();
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
        public string EventType { get; set; } = "SalesOrderCancelled";
        
        // Payload
        public Guid SalesOrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }
}
