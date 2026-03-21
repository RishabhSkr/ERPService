/*
 * StockReservedConsumer - Handles Inventory reservation response
 * 
 * 📚 Handles BOTH:
 *   - PO-level: WorkOrderId = null → updates ProductionOrder
 *   - WO-level: WorkOrderId has value → updates WorkOrder
 * 
 * Sub-State Pattern:
 *   On Success → ReservationStatus = Reserved (can activate/start)
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
                "Received StockReservedEvent: POId={POId}, WOId={WOId}, Success={Success}",
                @event.ProductionOrderId, @event.WorkOrderId, @event.Success);

            // ============================================================
            // ROUTE: WorkOrderId present → WO-level, otherwise PO-level
            // ============================================================
            if (@event.WorkOrderId.HasValue)
            {
                await HandleWorkOrderReservation(@event);
            }
            else
            {
                await HandleProductionOrderReservation(@event);
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 🆕 WO-level: Update WorkOrder reservation status
        /// </summary>
        private async Task HandleWorkOrderReservation(StockReservedEvent @event)
        {
            var wo = await _context.WorkOrders
                .FirstOrDefaultAsync(w => w.WorkOrderId == @event.WorkOrderId);

            if (wo == null)
            {
                _logger.LogWarning("WorkOrder not found: {WorkOrderId}", @event.WorkOrderId);
                return;
            }

            // Only process if WO is Released + Pending reservation
            if (wo.Status != WorkOrderStatus.Released ||
                wo.ReservationStatus != ReservationStatus.Pending)
            {
                _logger.LogWarning(
                    "Ignoring StockReservedEvent for WO {WONumber} — Status={Status}, Reservation={Reservation}",
                    wo.WorkOrderNumber, wo.Status, wo.ReservationStatus);
                return;
            }

            if (@event.Success)
            {
                wo.ReservationStatus = ReservationStatus.Reserved;
                wo.UpdatedAt = DateTime.UtcNow;

                _logger.LogInformation(
                    "✅ WO {WONumber} — materials reserved, ready to activate",
                    wo.WorkOrderNumber);
            }
            else
            {
                wo.ReservationStatus = ReservationStatus.Failed;
                wo.ReservationFailReason = @event.FailureReason ?? "Unknown failure";
                wo.UpdatedAt = DateTime.UtcNow;

                _logger.LogWarning(
                    "❌ Reservation failed for WO {WONumber}: {Reason}",
                    wo.WorkOrderNumber, @event.FailureReason);
            }
        }

        /// <summary>
        /// Existing PO-level: Update ProductionOrder (untouched, backward compatible)
        /// </summary>
        private async Task HandleProductionOrderReservation(StockReservedEvent @event)
        {
            var order = await _context.ProductionOrders
                .Include(o => o.MaterialRequirements)
                .FirstOrDefaultAsync(o => o.Id == @event.ProductionOrderId);

            if (order == null)
            {
                _logger.LogWarning("ProductionOrder not found: {ProductionOrderId}", @event.ProductionOrderId);
                return;
            }

            if (order.Status != ProductionOrderStatus.Released ||
                order.ReservationStatus != ReservationStatus.Pending)
            {
                _logger.LogWarning(
                    "Ignoring StockReservedEvent for {OrderNumber} — Status={Status}, Reservation={Reservation}",
                    order.OrderNumber, order.Status, order.ReservationStatus);
                return;
            }

            if (@event.Success)
            {
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
                order.ReservationStatus = ReservationStatus.Failed;
                order.ReservationFailReason = @event.FailureReason ?? "Unknown failure";
                order.UpdatedAt = DateTime.UtcNow;

                _logger.LogWarning(
                    "❌ Reservation failed for {OrderNumber}: {Reason}",
                    order.OrderNumber, @event.FailureReason);
            }
        }
    }
}
