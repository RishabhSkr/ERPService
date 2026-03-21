using MassTransit;
using MyERP.Shared.Events;
using MyERP.Services.Inventory.Data;
using MyERP.Services.Inventory.DTOs.StockMovements;
using MyERP.Services.Inventory.Services.StockMovements;
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
        _logger.LogInformation("Received material return request for PO: {OrderNumber}", @event.ProductionOrderNumber);

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
            // Find original RESERVE movement (to get WarehouseId)
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
                WarehouseId   = reservation.WarehouseId,
                Quantity      = reservation.Quantity,  // Release exactly what was reserved
                ReferenceType = "ProductionOrder",
                ReferenceId   = @event.ProductionOrderId,
                Notes         = $"Released - PO cancelled: {@event.ProductionOrderNumber}"
            });

            _logger.LogInformation("Released {Qty} of {MaterialCode} for cancelled PO {OrderNumber}",
                reservation.Quantity, material.MaterialCode, @event.ProductionOrderNumber);
        }

        _logger.LogInformation("Material return completed for PO: {OrderNumber}", @event.ProductionOrderNumber);
    }
}