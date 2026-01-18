using MyERP.Services.Sales.Events;

namespace MyERP.Services.Sales.Services.External
{
    /// <summary>
    /// Interface for publishing events to message broker (RabbitMQ).
    /// Currently logs events; will be implemented with RabbitMQ in Phase 2.
    /// </summary>
    public interface IEventPublisher
    {
        Task PublishAsync<T>(T @event) where T : class;
    }

    /// <summary>
    /// Placeholder event publisher that logs events.
    /// Will be replaced with RabbitMQ implementation in MyERP.Messaging library.
    /// </summary>
    public class LoggingEventPublisher : IEventPublisher
    {
        private readonly ILogger<LoggingEventPublisher> _logger;

        public LoggingEventPublisher(ILogger<LoggingEventPublisher> logger)
        {
            _logger = logger;
        }

        public Task PublishAsync<T>(T @event) where T : class
        {
            var eventName = typeof(T).Name;
            _logger.LogInformation(
                "📤 EVENT PUBLISHED: {EventName} | Payload: {@Event}",
                eventName, @event);
            
            // TODO: Replace with actual RabbitMQ publishing
            // await _rabbitMqChannel.BasicPublish(exchange, routingKey, body);
            
            return Task.CompletedTask;
        }
    }
}
