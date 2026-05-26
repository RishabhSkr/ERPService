using MassTransit;
using MyERP.SalesServiceTutorial.Events;

namespace MyERP.SalesServiceTutorial.Events.Publishers.MassTransit;

public class MassTransitEventPublisher : IEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<MassTransitEventPublisher> _logger;
    
    public MassTransitEventPublisher(
        IPublishEndpoint publishEndpoint, 
        ILogger<MassTransitEventPublisher> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }
    
    public async Task PublishAsync<T>(T @event) where T : BaseEvent
    {
        try
        {
            _logger.LogInformation("ðŸš€ [MassTransit] Publishing {EventType} event", typeof(T).Name);
            await _publishEndpoint.Publish(@event);
            _logger.LogInformation("âœ… Event published successfully!");
        }catch (Exception e)
        {
            _logger.LogError(e, "âŒ Failed to publish event {EventType}", typeof(T).Name);
            throw; // Re-throw so controller knows about error
        }
        
    }
}
