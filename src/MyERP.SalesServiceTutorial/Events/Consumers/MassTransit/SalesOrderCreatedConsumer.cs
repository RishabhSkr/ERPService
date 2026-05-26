using MassTransit;
using MyERP.SalesServiceTutorial.Events;

namespace MyERP.SalesServiceTutorial.Events.Consumers.MassTransit;

public class SalesOrderCreatedConsumer : IConsumer<SalesOrderCreatedEvent>
{
    private readonly ILogger<SalesOrderCreatedConsumer> _logger;

    public SalesOrderCreatedConsumer(ILogger<SalesOrderCreatedConsumer> logger)
    {
        _logger = logger;
    }
    public Task Consume(ConsumeContext<SalesOrderCreatedEvent> context)
    {
        var order = context.Message;
        
        _logger.LogInformation(
            "ðŸ“¦ [MassTransit] ORDER RECEIVED! OrderId: {OrderId}, CustomerId: {CustomerId}, Amount: {Amount}",
            order.OrderId,
            order.CustomerId,
            order.TotalAmount);
        
        // Process the event (update inventory, send email, etc.)
        _logger.LogInformation("âœ… Order {OrderId} processed successfully!", order.OrderId);
        
        return Task.CompletedTask;
    }
}