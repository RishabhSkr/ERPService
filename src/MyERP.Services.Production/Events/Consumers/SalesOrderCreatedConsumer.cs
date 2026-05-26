/*
 * SalesOrderCreatedConsumer - Handles Sales Order events
 * 
 * ðŸ“š INDUSTRY vs NOOB:
 * 
 * âŒ NOOB: Process immediately, no error handling, lose messages on failure
 * âœ… INDUSTRY:
 *    1. Inbox Pattern - save to DB first, process later
 *    2. Idempotency check - prevent duplicate processing
 *    3. Structured logging with correlation IDs
 *    4. MassTransit handles retries and DLQ automatically
 */

using MassTransit;
using Microsoft.EntityFrameworkCore;
using MyERP.Services.Production.Constants;
using MyERP.Services.Production.Data;
using MyERP.Shared.Events;  
using MyERP.Services.Production.Models;

namespace MyERP.Services.Production.Events.Consumers
{
    public class SalesOrderCreatedConsumer : IConsumer<SalesOrderCreatedEvent>
    {
        private readonly ProductionDbContext _context;
        private readonly ILogger<SalesOrderCreatedConsumer> _logger;

        public SalesOrderCreatedConsumer(
            ProductionDbContext context,
            ILogger<SalesOrderCreatedConsumer> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<SalesOrderCreatedEvent> context)
        {   
            Console.WriteLine("\nðŸŸ£ [DEBUG 8] Production Consumer - Consume() method called!");
            Console.WriteLine($"   Message ID: {context.MessageId}");
            
            var @event = context.Message;
            
            Console.WriteLine($"ðŸŸ£ [DEBUG 9] Event received:");
            Console.WriteLine($"   EventId: {@event.EventId}");
            Console.WriteLine($"   OrderNumber: {@event.OrderNumber}");
            Console.WriteLine($"   SalesOrderId: {@event.SalesOrderId}");
            Console.WriteLine($"   CustomerName: {@event.CustomerName}");
            Console.WriteLine($"   Items Count: {@event.Items?.Count ?? 0}");
            
            _logger.LogInformation(
                "Received SalesOrderCreatedEvent: OrderNumber={OrderNumber}, EventId={EventId}",
                @event.OrderNumber,
                @event.EventId);

            // ====================================
            // IDEMPOTENCY CHECK
            // ====================================
            Console.WriteLine($"ðŸŸ£ [DEBUG 10] Checking idempotency...");
            
            var alreadyProcessed = await _context.PendingRequests
                .AnyAsync(r => r.EventId == @event.EventId);
                
            if (alreadyProcessed)
            {
                Console.WriteLine($"ðŸŸ¡ [DEBUG] Duplicate event - already processed!");
                _logger.LogWarning(
                    "Duplicate event detected: EventId={EventId}, already processed",
                    @event.EventId);
                return;
            }

            Console.WriteLine($"ðŸŸ£ [DEBUG 11] Creating PendingRequest...");

            // ====================================
            // CREATE PENDING REQUEST (Inbox Pattern)
            // ====================================
            
            var pendingRequest = new PendingRequest
            {
                Id = Guid.NewGuid(),
                EventId = @event.EventId,
                SalesOrderId = @event.SalesOrderId,
                SalesOrderNumber = @event.OrderNumber,
                CustomerId = @event.CustomerId,
                CustomerName = @event.CustomerName,
                OrderDate = @event.OrderDate,
                Status = PendingRequestStatus.Pending,
                ReceivedAt = DateTime.UtcNow,
                Items = @event.Items.Select(i => new PendingRequestItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = i.ProductId,
                    ProductCode = i.ProductCode,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            };

            Console.WriteLine($"ðŸŸ£ [DEBUG 12] Saving to database...");
            _context.PendingRequests.Add(pendingRequest);
            await _context.SaveChangesAsync();

            Console.WriteLine($"ðŸŸ¢ [DEBUG 13] PendingRequest saved successfully!");
            Console.WriteLine($"   PendingRequest ID: {pendingRequest.Id}");
            
            _logger.LogInformation(
                "Created PendingRequest: Id={Id}, SalesOrderNumber={OrderNumber}, ItemCount={ItemCount}",
                pendingRequest.Id,
                pendingRequest.SalesOrderNumber,
                pendingRequest.Items.Count);
        }
    }
}
