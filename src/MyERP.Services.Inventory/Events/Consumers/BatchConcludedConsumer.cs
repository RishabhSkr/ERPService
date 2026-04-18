using MassTransit;
using MyERP.Shared.Events;
using MyERP.Services.Inventory.Data;
using MyERP.Services.Inventory.DTOs.StockMovements;
using MyERP.Services.Inventory.Services.StockMovements;
using MyERP.Services.Inventory.Constants;
using Microsoft.EntityFrameworkCore;

namespace MyERP.Services.Inventory.Events.Consumers;

/// <summary>
/// Handles COMPLETE flow — when production batch/WO is finished:
/// 1. Consume raw materials (CurrentStock -= consumed, ReservedStock -= reserved)
/// 2. Add finished goods to Product inventory (CurrentStock += QuantityGood)
/// 3. Handle product scrap
/// 4. Return unused materials
/// 
/// Supports both WO-level (WorkOrderId present) and PO-level (backward compatible)
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
        _logger.LogInformation(
            "Received batch concluded — PO: {PONumber}, WO: {WONumber}",
            @event.ProductionOrderNumber, @event.WorkOrderNumber ?? "N/A (PO-level)");

        // ─── Idempotency: check by WorkOrderId if WO-level, else by PO ───
        var alreadyProcessed = @event.WorkOrderId.HasValue
            ? await _context.StockMovements
                .AnyAsync(sm => sm.WorkOrderId == @event.WorkOrderId
                             && sm.MovementType == MovementType.OUT)
            : await _context.StockMovements
                .AnyAsync(sm => sm.ReferenceType == ReferenceType.PRODUCTION_ORDER
                             && sm.ReferenceId == @event.ProductionOrderId
                             && sm.WorkOrderId == null
                             && sm.MovementType == MovementType.OUT);

        if (alreadyProcessed)
        {
            _logger.LogWarning("Batch already processed for {Target} — skipping",
                @event.WorkOrderNumber ?? @event.ProductionOrderNumber);
            return;
        }

        var targetLabel = @event.WorkOrderId.HasValue
            ? $"WO: {@event.WorkOrderNumber}"
            : $"PO: {@event.ProductionOrderNumber}";

        // ─── Step 1: Consume raw materials ───
        foreach (var material in @event.MaterialsConsumed)
        {
            // Find original RESERVE movement — by WorkOrderId if WO-level
            var reservation = @event.WorkOrderId.HasValue
                ? await _context.StockMovements
                    .FirstOrDefaultAsync(sm => sm.WorkOrderId == @event.WorkOrderId
                                            && sm.ItemId == material.RawMaterialId
                                            && sm.MovementType == MovementType.RESERVE)
                : await _context.StockMovements
                    .FirstOrDefaultAsync(sm => sm.ReferenceType == ReferenceType.PRODUCTION_ORDER
                                            && sm.ReferenceId == @event.ProductionOrderId
                                            && sm.ItemId == material.RawMaterialId
                                            && sm.WorkOrderId == null
                                            && sm.MovementType == MovementType.RESERVE);

            var warehouseId = reservation?.WarehouseId ?? Guid.Empty;

            // A: Release reservation (ReservedStock -= consumed)
            if (reservation != null)
            {
                await _stockMovementService.RecordMovementAsync(new RecordStockMovementDto
                {
                    MovementType  = MovementType.RELEASE,
                    ItemType      = ItemType.RAW_MATERIAL,
                    ItemId        = material.RawMaterialId,
                    WarehouseId   = warehouseId,
                    Quantity      = material.QuantityConsumed,
                    ReferenceType = ReferenceType.PRODUCTION_ORDER,
                    ReferenceId   = @event.ProductionOrderId,
                    WorkOrderId   = @event.WorkOrderId,
                    Notes         = $"Un-reserve (consumed) for {targetLabel}",
                    CreatedAt     = DateTime.UtcNow,
                    CreatedBy     = SystemUser.Id
                });
            }

            // B: Consume from stock (CurrentStock -= consumed)
            await _stockMovementService.RecordMovementAsync(new RecordStockMovementDto
            {
                MovementType  = MovementType.OUT,
                ItemType      = ItemType.RAW_MATERIAL,
                ItemId        = material.RawMaterialId,
                WarehouseId   = warehouseId,
                Quantity      = material.QuantityConsumed,
                ReferenceType = ReferenceType.PRODUCTION_ORDER,
                ReferenceId   = @event.ProductionOrderId,
                WorkOrderId   = @event.WorkOrderId,
                Notes         = $"Consumed in production: {targetLabel}",
                CreatedAt     = DateTime.UtcNow,
                CreatedBy     = SystemUser.Id
            });

            _logger.LogInformation("Consumed {Qty} of {MaterialCode} for {Target}",
                material.QuantityConsumed, material.MaterialCode, targetLabel);

            // C: Return unused material (if any)
            if (material.QuantityReturned > 0)
            {
                await _stockMovementService.RecordMovementAsync(new RecordStockMovementDto
                {
                    MovementType  = MovementType.RELEASE,
                    ItemType      = ItemType.RAW_MATERIAL,
                    ItemId        = material.RawMaterialId,
                    WarehouseId   = warehouseId,
                    Quantity      = material.QuantityReturned,
                    ReferenceType = ReferenceType.PRODUCTION_ORDER,
                    ReferenceId   = @event.ProductionOrderId,
                    WorkOrderId   = @event.WorkOrderId,
                    Notes         = $"Unused material returned: {targetLabel}",
                    CreatedAt     = DateTime.UtcNow,
                    CreatedBy     = SystemUser.Id
                });

                _logger.LogInformation("Returned {Qty} unused {MaterialCode} for {Target}",
                    material.QuantityReturned, material.MaterialCode, targetLabel);
            }
        }

        // ─── Step 2: Add finished goods to Product inventory ───
        if (@event.QuantityGood > 0)
        {
            var warehouse = await _context.Warehouses.FirstOrDefaultAsync();
            if (warehouse != null)
            {
                await _stockMovementService.RecordMovementAsync(new RecordStockMovementDto
                {
                    MovementType  = MovementType.IN,
                    ItemType      = ItemType.PRODUCT,
                    ItemId        = @event.ProductId,
                    WarehouseId   = warehouse.Id,
                    Quantity      = @event.QuantityGood,
                    ReferenceType = ReferenceType.PRODUCTION_ORDER,
                    ReferenceId   = @event.ProductionOrderId,
                    WorkOrderId   = @event.WorkOrderId,
                    Notes         = $"Finished goods from: {targetLabel}",
                    CreatedAt     = DateTime.UtcNow,
                    CreatedBy     = SystemUser.Id
                });

                _logger.LogInformation("Added {Qty} finished goods of {ProductCode} for {Target}",
                    @event.QuantityGood, @event.ProductCode, targetLabel);
            }
        }

        // ─── Step 3: Handle product scrap ───
        if (@event.QuantityScrap > 0)
        {
            var warehouse = await _context.Warehouses.FirstOrDefaultAsync();
            if (warehouse != null)
            {
                await _stockMovementService.RecordMovementAsync(new RecordStockMovementDto
                {
                    MovementType  = MovementType.SCRAP,
                    ItemType      = ItemType.PRODUCT,
                    ItemId        = @event.ProductId,
                    WarehouseId   = warehouse.Id,
                    Quantity      = @event.QuantityScrap,
                    ReferenceType = ReferenceType.PRODUCTION_ORDER,
                    ReferenceId   = @event.ProductionOrderId,
                    WorkOrderId   = @event.WorkOrderId,
                    Notes         = $"Scrap from production: {targetLabel}",
                    CreatedAt     = DateTime.UtcNow,
                    CreatedBy     = SystemUser.Id
                });
            }
        }

        _logger.LogInformation("Batch concluded processing complete for {Target}", targetLabel);
    }
}
