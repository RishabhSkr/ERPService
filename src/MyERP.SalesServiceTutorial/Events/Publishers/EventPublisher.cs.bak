using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace MyERP.SalesServiceTutorial.Events.Publishers;

public class EventPublisher : IEventPublisher
{
    private readonly ILogger<EventPublisher> _logger;
    private readonly IConfiguration _configuration;
    
    public EventPublisher(ILogger<EventPublisher> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }
    
    public async Task PublishAsync<T>(T @event) where T : BaseEvent
    {
        var eventName = typeof(T).Name;
        var exchangeName = "sales-events";
        
        try
        {
            // Create connection factory
            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMQ:HostName"] ?? "localhost",
                Port = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672"),
                UserName = _configuration["RabbitMQ:UserName"] ?? "guest",
                Password = _configuration["RabbitMQ:Password"] ?? "guest"
            };
            
            // Create connection and channel
            await using var connection = await factory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();
            
            // Declare exchange (topic type for routing)
            await channel.ExchangeDeclareAsync(
                exchange: exchangeName,
                type: ExchangeType.Topic,
                durable: true);
            
            // Serialize event to JSON
            var eventJson = JsonSerializer.Serialize(@event);
            var body = Encoding.UTF8.GetBytes(eventJson);
            
            // Publish message
            await channel.BasicPublishAsync(
                exchange: exchangeName,
                routingKey: eventName,  // e.g., "SalesOrderCreatedEvent"
                body: body);
            
            _logger.LogInformation(
                "📢 EVENT PUBLISHED to RabbitMQ: {EventName}\n{EventJson}", 
                eventName, 
                eventJson);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish event {EventName}", eventName);
            throw;
        }
    }
}