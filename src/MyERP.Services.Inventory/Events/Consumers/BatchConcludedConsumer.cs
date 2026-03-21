using MassTransit;
using MyERP.Shared.Events;
using MyERP.Services.Inventory.Data;
using MyERP.Services.Inventory.DTOs.StockMovements;
using MyERP.Services.Inventory.Services.StockMovements;
using Microsoft.EntityFrameworkCore;

namespace MyERP.Services.Inventory.Events.Consumers;

/// <summary>
/// Handles COMPLETE flow — when production batch is finished:
/// 1. Consume raw materials (CurrentStock -= consumed, ReservedStock -= reserved)
/// 2. Add finished goods to Product inventory (CurrentStock += QuantityGood)
/// </summary>
public class BatchConcludedConsumer : IConsumer<BatchConcludedEvent>
{
    private readonly InventoryDbContext _context;
    private readonly IStockMovementService _stockMovementService;
    private readonly ILogger<BatchConcludedConsumer> _logger;

    public BatchConcludedConsumer(
        InventoryDbContext context,
        IStockMovementService stockMovementService,
        ILogger<BatchConcludedConsumer> logger)
    {
        _context = context;
        _stockMovementService = stockMovementService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<BatchConcludedEvent> context)
    {
        var @event = context.Message;
        _logger.LogInformation("Received batch concluded for PO: {OrderNumber}", @event.ProductionOrderNumber);

        // ─── Idempotency: check if already processed ───
        var alreadyProcessed = await _context.StockMovements
            .AnyAsync(sm => sm.ReferenceType == "ProductionOrder"
                         && sm.ReferenceId == @event.ProductionOrderId
                         && sm.MovementType == "OUT");
        if (alreadyProcessed)
        {
            _logger.LogWarning("Batch already processed for PO {OrderNumber} — skipping",
                @event.ProductionOrderNumber);
            return;
        }

        // ─── Step 1: Consume raw materials ───
        foreach (var material in @event.MaterialsConsumed)
        {
            // Find original RESERVE movement (for WarehouseId)
            var reservation = await _context.StockMovements
                .FirstOrDefaultAsync(sm => sm.ReferenceType == "ProductionOrder"
                                        && sm.ReferenceId == @event.ProductionOrderId
                                        && sm.ItemId == material.RawMaterialId
                                        && sm.MovementType == "RESERVE");

            var warehouseId = reservation?.WarehouseId ?? Guid.Empty;

            // A: Release reservation (ReservedStock -= consumed)
            if (reservation != null)
            {
                await _stockMovementService.RecordMovementAsync(new RecordStockMovementDto
                {
                    MovementType  = "RELEASE",
                    ItemType      = "RawMaterial",
                    ItemId        = material.RawMaterialId,
                    WarehouseId   = warehouseId,
                    Quantity      = material.QuantityConsumed,
                    ReferenceType = "ProductionOrder",
                    ReferenceId   = @event.ProductionOrderId,
                    Notes         = $"Un-reserve (consumed) for {@event.ProductionOrderNumber}"
                });
            }

            // B: Consume from stock (CurrentStock -= consumed)
            await _stockMovementService.RecordMovementAsync(new RecordStockMovementDto
            {
                MovementType  = "OUT",
                ItemType      = "RawMaterial",
                ItemId        = material.RawMaterialId,
                WarehouseId   = warehouseId,
                Quantity      = material.QuantityConsumed,
                ReferenceType = "ProductionOrder",
                ReferenceId   = @event.ProductionOrderId,
                Notes         = $"Consumed in production: {@event.ProductionOrderNumber}"
            });

            _logger.LogInformation("Consumed {Qty} of {MaterialCode} for PO {OrderNumber}",
                material.QuantityConsumed, material.MaterialCode, @event.ProductionOrderNumber);

            // C: Return unused material (if any)
            if (material.QuantityReturned > 0)
            {
                await _stockMovementService.RecordMovementAsync(new RecordStockMovementDto
                {
                    MovementType  = "RELEASE",
                    ItemType      = "RawMaterial",
                    ItemId        = material.RawMaterialId,
                    WarehouseId   = warehouseId,
                    Quantity      = material.QuantityReturned,
                    ReferenceType = "ProductionOrder",
                    ReferenceId   = @event.ProductionOrderId,
                    Notes         = $"Unused material returned: {@event.ProductionOrderNumber}"
                });
            }
        }

        // ─── Step 2: Add finished goods to Product inventory ───
        if (@event.QuantityGood > 0)
        {
            // Find any warehouse for this product (or use first warehouse)
            var warehouse = await _context.Warehouses.FirstOrDefaultAsync();
            if (warehouse != null)
            {
                await _stockMovementService.RecordMovementAsync(new RecordStockMovementDto
                {
                    MovementType  = "IN",
                    ItemType      = "Product",
                    ItemId        = @event.ProductId,
                    WarehouseId   = warehouse.Id,
                    Quantity      = @event.QuantityGood,
                    ReferenceType = "ProductionOrder",
                    ReferenceId   = @event.ProductionOrderId,
                    Notes         = $"Finished goods from: {@event.ProductionOrderNumber}"
                });

                _logger.LogInformation("Added {Qty} finished goods of {ProductCode} for PO {OrderNumber}",
                    @event.QuantityGood, @event.ProductCode, @event.ProductionOrderNumber);
            }
        }

        // ─── Step 3: Handle scrap ───
        if (@event.QuantityScrap > 0)
        {
            var warehouse = await _context.Warehouses.FirstOrDefaultAsync();
            if (warehouse != null)
            {
                await _stockMovementService.RecordMovementAsync(new RecordStockMovementDto
                {
                    MovementType  = "SCRAP",
                    ItemType      = "Product",
                    ItemId        = @event.ProductId,
                    WarehouseId   = warehouse.Id,
                    Quantity      = @event.QuantityScrap,
                    ReferenceType = "ProductionOrder",
                    ReferenceId   = @event.ProductionOrderId,
                    Notes         = $"Scrap from production: {@event.ProductionOrderNumber}"
                });
            }
        }

        _logger.LogInformation("Batch concluded processing complete for PO: {OrderNumber}", @event.ProductionOrderNumber);
    }
}
