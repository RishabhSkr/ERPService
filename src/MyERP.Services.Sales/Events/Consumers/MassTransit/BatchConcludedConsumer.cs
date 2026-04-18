using MassTransit;
using Microsoft.EntityFrameworkCore;
using MyERP.Services.Sales.Data;
using MyERP.Services.Sales.Constants;
using MyERP.Shared.Events;

namespace MyERP.Services.Sales.Events.Consumers
{
    /// <summary>
    /// Consumes BatchConcludedEvent from Production.
    /// Updates SalesOrderItem.QuantityProduced when a WO completes.
    /// Real-time tracking: each WO completion adds to produced qty.
    /// </summary>
    public class BatchConcludedConsumer : IConsumer<BatchConcludedEvent>
    {
        private readonly SalesDbContext _context;
        private readonly ILogger<BatchConcludedConsumer> _logger;

        public BatchConcludedConsumer(
            SalesDbContext context,
            ILogger<BatchConcludedConsumer> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<BatchConcludedEvent> context)
        {
            var @event = context.Message;

            // Skip if not linked to a Sales Order
            if (!@event.SalesOrderId.HasValue)
            {
                _logger.LogInformation(
                    "BatchConcluded for PO {PONumber} — no SalesOrderId, skipping",
                    @event.ProductionOrderNumber);
                return;
            }

            // Find matching SalesOrderItem by ProductId
            var salesOrderItem = await _context.SalesOrderItems
                .FirstOrDefaultAsync(i => i.SalesOrderId == @event.SalesOrderId.Value
                                       && i.ProductId == @event.ProductId);

            if (salesOrderItem == null)
            {
                _logger.LogWarning(
                    "No SalesOrderItem found for SalesOrderId={SalesOrderId}, ProductId={ProductId}",
                    @event.SalesOrderId, @event.ProductId);
                return;
            }

            // Update produced quantity
            salesOrderItem.QuantityProduced += @event.QuantityGood;

            // Auto-update Sales Order status to InProduction (if still Confirmed)
            var salesOrder = await _context.SalesOrders.FindAsync(@event.SalesOrderId.Value);
            if (salesOrder != null && salesOrder.OrderStatus == SalesOrderStatus.CONFIRMED)
            {
                salesOrder.OrderStatus = SalesOrderStatus.IN_PRODUCTION;
                salesOrder.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Updated fulfillment: SO={SalesOrderId}, Product={ProductCode}, Produced={Qty}",
                @event.SalesOrderId, @event.ProductCode, salesOrderItem.QuantityProduced);
        }
    }
}
