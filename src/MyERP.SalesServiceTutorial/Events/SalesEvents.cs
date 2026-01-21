namespace MyERP.SalesServiceTutorial.Events;

// Base event class
public abstract class BaseEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

public class SalesOrderCreatedEvent : BaseEvent
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime OrderDate { get; set; }
}

public class SalesOrderCancelledEvent : BaseEvent
{
    public int OrderId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime CancelledAt { get; set; }
}
