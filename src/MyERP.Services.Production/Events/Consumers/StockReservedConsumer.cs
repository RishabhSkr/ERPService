/*
 * StockReservedConsumer - Handles Inventory reservation response
 * 
 * 📚 Sub-State Pattern:
 *   On Success → ReservationStatus = Reserved (can Start production)
 *   On Failure → ReservationStatus = Failed (user can Retry)
 */

using MassTransit;
using Microsoft.EntityFrameworkCore;
using MyERP.Services.Production.Constants;
using MyERP.Services.Production.Data;
using MyERP.Services.Production.Events;
using MyERP.Shared.Events;
using MyERP.Services.Production.Models;

namespace MyERP.Services.Production.Events.Consumers
{
    public class StockReservedConsumer : IConsumer<StockReservedEvent>
    {
        private readonly ProductionDbContext _context;
        private readonly ILogger<StockReservedConsumer> _logger;

        public StockReservedConsumer(
            ProductionDbContext context,
            ILogger<StockReservedConsumer> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<StockReservedEvent> context)
        {
            var @event = context.Message;
            
            _logger.LogInformation(
                "Received StockReservedEvent: ProductionOrderId={ProductionOrderId}, Success={Success}",
                @event.ProductionOrderId,
                @event.Success);

            // Find the production order with materials
            var order = await _context.ProductionOrders
                .Include(o => o.MaterialRequirements)
                .FirstOrDefaultAsync(o => o.Id == @event.ProductionOrderId);

            if (order == null)
            {
                _logger.LogWarning("ProductionOrder not found: {ProductionOrderId}", @event.ProductionOrderId);
                return;
            }

            // Only process if order is Released + Pending
            if (order.Status != ProductionOrderStatus.Released ||
                order.ReservationStatus != ReservationStatus.Pending)
            {
                _logger.LogWarning(
                    "Ignoring StockReservedEvent for {OrderNumber} — Status={Status}, ReservationStatus={ReservationStatus}",
                    order.OrderNumber, order.Status, order.ReservationStatus);
                return;
            }

            if (@event.Success)
            {
                // ✅ SUCCESS: Update material quantities + set Reserved
                foreach (var reserved in @event.ReservedMaterials)
                {
                    var requirement = order.MaterialRequirements
                        .FirstOrDefault(r => r.RawMaterialId == reserved.RawMaterialId);
                    
                    if (requirement != null)
                    {
                        requirement.QuantityReserved = reserved.QuantityReserved;
                        requirement.Status = "Reserved";
                    }
                }

                order.ReservationStatus = ReservationStatus.Reserved;
                order.UpdatedAt = DateTime.UtcNow;

                _logger.LogInformation(
                    "✅ ProductionOrder {OrderNumber} — materials reserved, ready to start",
                    order.OrderNumber);
            }
            else
            {
                // ❌ FAILURE: Set Failed + store reason for user to see
                order.ReservationStatus = ReservationStatus.Failed;
                order.ReservationFailReason = @event.FailureReason ?? "Unknown failure";
                order.UpdatedAt = DateTime.UtcNow;

                _logger.LogWarning(
                    "❌ Reservation failed for {OrderNumber}: {Reason}",
                    order.OrderNumber,
                    @event.FailureReason);
            }

            await _context.SaveChangesAsync();
        }
    }
}
