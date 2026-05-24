using MassTransit;
using Microsoft.EntityFrameworkCore;
using MyERP.Services.Sales.Data;
using MyERP.Shared.Events;

namespace MyERP.Services.Sales.Events.Consumers
{
    /// <summary>
    /// Consumes ProductionOrderCancelledEvent from Production.
    /// Notifies Sales Order that linked PO was cancelled.
    /// Does NOT auto-cancel Sales Order — manager decides.
    /// </summary>
    public class ProductionCancelledConsumer : IConsumer<ProductionOrderCancelledEvent>
    {
        private readonly SalesDbContext _context;
        private readonly ILogger<ProductionCancelledConsumer> _logger;

        public ProductionCancelledConsumer(
            SalesDbContext context,
            ILogger<ProductionCancelledConsumer> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ProductionOrderCancelledEvent> context)
        {
            var @event = context.Message;

            // Skip if not linked to a Sales Order
            if (!@event.SalesOrderId.HasValue)
            {
                _logger.LogInformation(
                    "PO {PONumber} cancelled — no SalesOrderId, skipping",
                    @event.ProductionOrderNumber);
                return;
            }

            var salesOrder = await _context.SalesOrders
                .FindAsync(@event.SalesOrderId.Value);

            if (salesOrder == null)
            {
                _logger.LogWarning(
                    "SalesOrder not found: {SalesOrderId}", @event.SalesOrderId);
                return;
            }

            // Add note — DON'T auto-cancel (manager decides)
            salesOrder.Notes = string.IsNullOrEmpty(salesOrder.Notes)
                ? $"⚠️ Production cancelled: PO {@event.ProductionOrderNumber} — {@event.Reason}"
                : $"{salesOrder.Notes}\n⚠️ Production cancelled: PO {@event.ProductionOrderNumber} — {@event.Reason}";
            salesOrder.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogWarning(
                "Production cancelled for SalesOrder {OrderNumber}: PO={PONumber}, Reason={Reason}",
                salesOrder.OrderNumber, @event.ProductionOrderNumber, @event.Reason);
        }
    }
}
