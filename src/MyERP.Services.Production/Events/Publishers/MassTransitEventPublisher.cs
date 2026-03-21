/*
 * MassTransit Event Publisher Implementation
 */

using MassTransit;

namespace MyERP.Services.Production.Events.Publishers
{
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

        public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) 
            where T : class
        {
            _logger.LogInformation("Publishing event {EventType}: {@Event}", typeof(T).Name, @event);
            
            await _publishEndpoint.Publish(@event, cancellationToken);
            
            _logger.LogInformation("Event {EventType} published successfully", typeof(T).Name);
        }
    }
}
