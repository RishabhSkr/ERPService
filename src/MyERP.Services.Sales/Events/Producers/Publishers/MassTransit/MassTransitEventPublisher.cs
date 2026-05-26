using MyERP.Services.Sales.Events.Producers.Publishers.MassTransit;
using MassTransit;

namespace MyERP.Services.Sales.Events.Producers.Publishers.MassTransit;

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
    
    public async Task PublishAsync<T>(T @event) where T : class
    {
        Console.WriteLine($"\nðŸŸ¡ [DEBUG 5] MassTransitEventPublisher.PublishAsync() called");
        Console.WriteLine($"   Event Type: {typeof(T).Name}");
        Console.WriteLine($"   Event Full Name: {typeof(T).FullName}");
        
        try
        {
            Console.WriteLine($"ðŸŸ¡ [DEBUG 6] Calling _publishEndpoint.Publish()...");
            _logger.LogInformation("ðŸš€ [MassTransit] Publishing {EventType} event", typeof(T).Name);
            
            await _publishEndpoint.Publish(@event);
            
            Console.WriteLine($"ðŸŸ¢ [DEBUG 7] _publishEndpoint.Publish() completed!");
            _logger.LogInformation("âœ… Event published successfully!");
        }
        catch (Exception e)
        {
            Console.WriteLine($"ðŸ”´ [DEBUG ERROR] Exception in PublishAsync: {e.Message}");
            _logger.LogError(e, "âŒ Failed to publish event {EventType}", typeof(T).Name);
            throw;
        }
        
    }
}
