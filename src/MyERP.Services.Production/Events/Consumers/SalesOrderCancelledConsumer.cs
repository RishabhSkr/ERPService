using MassTransit;
using Microsoft.EntityFrameworkCore;
using MyERP.Services.Production.Data;
using MyERP.Services.Production.Services.WorkOrder;
using MyERP.Shared.Events;

namespace MyERP.Services.Production.Events.Consumers
{
    public class SalesOrderCancelledConsumer : IConsumer<SalesOrderCancelledEvent>
    {
        // Step 1: Dependencies inject karo
        // 1. DB Context — PendingRequest + PO direct query ke liye
        private readonly ProductionDbContext _context;
        // 2. WO Service — WO cancel karne ke liye (saga trigger hoga)
        private readonly IWorkOrderService _woService;

        // 3. Logger — structured logging
        private readonly ILogger<SalesOrderCancelledConsumer> _logger;

        // Step 2: Constructor
        public SalesOrderCancelledConsumer(
            ProductionDbContext context,
            IWorkOrderService woService,
            ILogger<SalesOrderCancelledConsumer> logger)
        {
            _context = context;
            _woService = woService;
            _logger = logger;
        }

        // Step 3: Consume method
        public async Task Consume(ConsumeContext<SalesOrderCancelledEvent> context)
        {
            var @event = context.Message;
            _logger.LogInformation(
                "Received SalesOrderCancelledEvent: {OrderNumber}, Reason: {Reason}",
                @event.OrderNumber, @event.Reason);

            // ─── STEP 1: Cancel PendingRequest (if exists, not yet approved) ───
            var pendingRequest = await _context.PendingRequests
                .FirstOrDefaultAsync(r => r.SalesOrderId == @event.SalesOrderId
                                    && r.Status == Constants.PendingRequestStatus.Pending);
            if (pendingRequest != null)
            {
                pendingRequest.Status = Constants.PendingRequestStatus.Cancelled;
                await _context.SaveChangesAsync();
                _logger.LogInformation("Cancelled PendingRequest for SalesOrder {OrderNumber}", @event.OrderNumber);
            }

            // ─── STEP 2: Cancel Production Orders (linked to SalesOrderId) ───
            var productionOrders = await _context.ProductionOrders
                .Include(o => o.MaterialRequirements)
                .Where(o => o.SalesOrderId == @event.SalesOrderId
                        && o.Status != Constants.ProductionOrderStatus.Completed
                        && o.Status != Constants.ProductionOrderStatus.Cancelled)
                .ToListAsync();

            foreach (var po in productionOrders)
            {
                // ─── STEP 3: Cancel WOs under this PO (triggers material return saga) ───
                var workOrders = await _context.WorkOrders
                    .Where(w => w.ProductionOrderId == po.Id
                            && w.Status != Constants.WorkOrderStatus.Completed
                            && w.Status != Constants.WorkOrderStatus.Cancelled)
                    .ToListAsync();

                foreach (var wo in workOrders)
                {
                    try
                    {
                        await _woService.CancelAsync(wo.WorkOrderId,
                            $"Sales Order {@event.OrderNumber} cancelled: {@event.Reason}");
                        _logger.LogInformation("Cascade cancelled WO {WONumber}", wo.WorkOrderNumber);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to cancel WO {WONumber} — may already be cancelled",
                            wo.WorkOrderNumber);
                    }
                }

                // Cancel PO itself (direct DB — skip InProgress check for Sales-initiated)
                po.Status = Constants.ProductionOrderStatus.Cancelled;
                po.CancelReason = $"Sales Order {@event.OrderNumber} cancelled: {@event.Reason}";
                po.CancelledAt = DateTime.UtcNow;
                po.UpdatedAt = DateTime.UtcNow;
                _logger.LogInformation("Cascade cancelled PO {OrderNumber}", po.OrderNumber);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "SalesOrderCancelled processed: {OrderNumber} — {POCount} POs cancelled",
                @event.OrderNumber, productionOrders.Count);
        }

    }
}
