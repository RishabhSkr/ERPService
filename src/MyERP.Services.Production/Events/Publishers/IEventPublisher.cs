/*
 * Event Publisher Interface
 * 
 * ðŸ“š INDUSTRY vs NOOB:
 * 
 * âŒ NOOB: Publish directly via MassTransit in services
 * âœ… INDUSTRY:
 *    1. Abstraction layer over messaging
 *    2. Easy to mock in tests
 *    3. Can switch from RabbitMQ to Azure Service Bus without changing services
 */

namespace MyERP.Services.Production.Events.Publishers
{
    /// <summary>
    /// Abstraction for publishing events
    /// Allows swapping message broker without changing business logic
    /// </summary>
    public interface IEventPublisher
    {
        Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : class;
    }
}
