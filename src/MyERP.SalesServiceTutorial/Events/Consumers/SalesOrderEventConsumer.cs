using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MyERP.SalesServiceTutorial.Events.Consumers;

public class SalesOrderEventConsumer : BackgroundService
{
    private readonly ILogger<SalesOrderEventConsumer> _logger;
    private readonly IConfiguration _configuration;
    
    public SalesOrderEventConsumer(ILogger<SalesOrderEventConsumer> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🎧 Event Consumer starting...");
        
        // We'll add the RabbitMQ listening code here
        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMQ:HostName"] ?? "localhost",
            Port = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672"),
            UserName = _configuration["RabbitMQ:UserName"] ?? "guest",
            Password = _configuration["RabbitMQ:Password"] ?? "guest"
        };
        // 2. Connection & Channel banao
        var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();
        var exchangeName = "sales-events";

        await channel.ExchangeDeclareAsync(
            exchange: exchangeName,
            type: ExchangeType.Topic,
            durable: true);

        // 4. Declare Queue
        var queueName = "sales-order-events";
        await channel.QueueDeclareAsync(
            queue: queueName,
            durable: true,      // Queue survives restart
            exclusive: false,   // Other connections can use
            autoDelete: false); // Don't delete when no consumers

        // 5. Bind Queue to Exchange
        await channel.QueueBindAsync(
            queue: queueName,
            exchange: exchangeName,
            routingKey: "SalesOrderCreatedEvent");
        // 6. Create consumer(listener)
        var consumer = new AsyncEventingBasicConsumer(channel);
        // 7. Define what to d to when message arrives
        consumer.ReceivedAsync += async (sender, args) =>
        {
            var body = args.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            try
            {
                var salesOrder = JsonSerializer.Deserialize<SalesOrderCreatedEvent>(message);
                if (salesOrder != null)
                {
                    _logger.LogInformation(
                        "📦 ORDER RECEIVED! OrderId: {OrderId}, CustomerId: {CustomerId}, Amount: {Amount}",
                        salesOrder.OrderId,
                        salesOrder.CustomerId,
                        salesOrder.TotalAmount);
                    
                    // Process the event (Real-world examples):
                    // - Update inventory stock
                    // - Send confirmation email
                    
                    _logger.LogInformation("✅ Order {OrderId} processed successfully!", salesOrder.OrderId);
                }
                
                // Success - Acknowledge the message
                await channel.BasicAckAsync(args.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error processing order");
                // Send to dead letter (requeue: false)
                await channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: false);
            }
        };
        
        // 9. Start consuming
        await channel.BasicConsumeAsync(
            queue: queueName, 
            autoAck: false, 
            consumer: consumer);

        _logger.LogInformation("🎧 Event Consumer started...");


        // 10. Keep running until app stopped
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
        
        await Task.CompletedTask;
    }
}