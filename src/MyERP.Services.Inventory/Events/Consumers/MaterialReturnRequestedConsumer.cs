using MassTransit;
using MyERP.Shared.Events;
using MyERP.Services.Inventory.Data;
using MyERP.Services.Inventory.DTOs.StockMovements;
using MyERP.Services.Inventory.Services.StockMovements;
using MyERP.Services.Inventory.Constants;
using Microsoft.EntityFrameworkCore;

namespace MyERP.Services.Inventory.Events.Consumers;

/// <summary>
/// Handles CANCEL flow — releases reserved materials back to available stock
/// SAGA Compensating Action: Undo reservation when Production Order is cancelled
/// </summary>
public class MaterialReturnRequestedConsumer : IConsumer<MaterialReturnRequestedEvent>
{
    private readonly InventoryDbContext _context;
    private readonly IStockMovementService _stockMovementService;
    private readonly ILogger<MaterialReturnRequestedConsumer> _logger;

    public MaterialReturnRequestedConsumer(
        InventoryDbContext context,
        IStockMovementService stockMovementService,
        ILogger<MaterialReturnRequestedConsumer> logger)
    {
        _context = context;
        _stockMovementService = stockMovementService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<MaterialReturnRequestedEvent> context)
    {
        var @event = context.Message;
        _logger.LogInformation(
            "Material return request — PO: {PONumber}, WO: {WONumber}",
            @event.ProductionOrderNumber, @event.WorkOrderNumber ?? "N/A (PO-level)");
        // ROUTE: WorkOrderId present → WO-level, otherwise PO-level
        if (@event.WorkOrderId.HasValue)
        {
            await HandleWorkOrderReturn(@event);
        }
        else
        {
            await HandleProductionOrderReturn(@event);
        }
        
    }
    // WO-level: Find RESERVE by WorkOrderId → RELEASE
    private async Task HandleWorkOrderReturn(MaterialReturnRequestedEvent @event)
    {
        // Idempotency: already released for this WO?
        var alreadyReturned = await _context.StockMovements
            .AnyAsync(sm => sm.WorkOrderId == @event.WorkOrderId
                        && sm.MovementType == "RELEASE");
        if (alreadyReturned)
        {
            _logger.LogWarning("Return already processed for WO {WONumber}", @event.WorkOrderNumber);
            return;
        }
        // Find all RESERVE movements for this specific WO
        var reservations = await _context.StockMovements
            .Where(sm => sm.WorkOrderId == @event.WorkOrderId
                    && sm.MovementType == "RESERVE")
            .ToListAsync();
        if (!reservations.Any())
        {
            _logger.LogWarning("No reservations found for WO {WONumber}", @event.WorkOrderNumber);
            return;
        }
        foreach (var reservation in reservations)
        {
            await _stockMovementService.RecordMovementAsync(new RecordStockMovementDto
            {
                MovementType  = "RELEASE",
                ItemType      = "RawMaterial",
                ItemId        = reservation.ItemId,
                StorageLocationId   = reservation.FromLocationId ?? Guid.Empty,
                Quantity      = reservation.Quantity,  // Release exactly what was reserved
                ReferenceType = "ProductionOrder",
                ReferenceId   = @event.ProductionOrderId,
                WorkOrderId   = @event.WorkOrderId,    // KEY: WO tracking!
                Notes         = $"Released - WO cancelled: {@event.WorkOrderNumber}",
                CreatedAt     = DateTime.UtcNow,
                CreatedBy     = SystemUser.Id
            });
            _logger.LogInformation("Released {Qty} of Item {ItemId} for WO {WONumber}",
                reservation.Quantity, reservation.ItemId, @event.WorkOrderNumber);
        }
    }

    // PO-level: original logic (kept for backward compatibility)
    private async Task HandleProductionOrderReturn(MaterialReturnRequestedEvent @event)
    {
    
            // ─── Idempotency: check if RELEASE already done for this PO ───
            var alreadyReturned = await _context.StockMovements
                .AnyAsync(sm => sm.ReferenceType == "ProductionOrder"
                            && sm.ReferenceId == @event.ProductionOrderId
                            && sm.MovementType == "RELEASE");
            if (alreadyReturned)
            {
                _logger.LogWarning("Material return already processed for PO {OrderNumber} — skipping",
                    @event.ProductionOrderNumber);
                return;
            }

            // ─── Release each material ───
            foreach (var material in @event.MaterialsConsumed)
            {
                // Find original RESERVE movement (to get StorageLocationId)
                var reservation = await _context.StockMovements
                    .FirstOrDefaultAsync(sm => sm.ReferenceType == "ProductionOrder"
                                            && sm.ReferenceId == @event.ProductionOrderId
                                            && sm.ItemId == material.RawMaterialId
                                            && sm.MovementType == "RESERVE");
                if (reservation == null)
                {
                    _logger.LogWarning("No reservation found for {MaterialCode} in PO {OrderNumber} — skipping",
                        material.MaterialCode, @event.ProductionOrderNumber);
                    continue;
                }

                await _stockMovementService.RecordMovementAsync(new RecordStockMovementDto
                {
                    MovementType  = "RELEASE",
                    ItemType      = "RawMaterial",
                    ItemId        = material.RawMaterialId,
                    StorageLocationId   = reservation.FromLocationId ?? Guid.Empty,
                    Quantity      = reservation.Quantity,  // Release exactly what was reserved
                    ReferenceType = "ProductionOrder",
                    ReferenceId   = @event.ProductionOrderId,
                    Notes         = $"Released - PO cancelled: {@event.ProductionOrderNumber}",
                    CreatedAt     = DateTime.UtcNow,
                    CreatedBy     = SystemUser.Id 
                });

                _logger.LogInformation("Released {Qty} of {MaterialCode} for cancelled PO {OrderNumber}",
                    reservation.Quantity, material.MaterialCode, @event.ProductionOrderNumber);
            }

            _logger.LogInformation("Material return completed for PO: {OrderNumber}", @event.ProductionOrderNumber);
    }
}
