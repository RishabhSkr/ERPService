using MassTransit;
using MyERP.Shared.Events;
using MyERP.Services.Inventory.Data;
using MyERP.Services.Inventory.DTOs.StockMovements;
using MyERP.Services.Inventory.Repositories.RawMaterials;
using MyERP.Services.Inventory.Services.StockMovements;
using MyERP.Services.Inventory.Constants;
using Microsoft.EntityFrameworkCore;

namespace MyERP.Services.Inventory.Events.Consumers;

public class MaterialReservationRequestedConsumer : IConsumer<MaterialReservationRequestedEvent>
{
        private readonly InventoryDbContext _context;
        private readonly ILogger<MaterialReservationRequestedConsumer> _logger;
        private IPublishEndpoint _publishEndpoint;
        private readonly IRawMaterialRepository _rawMaterialRepo;       
        private readonly IStockMovementService _stockMovementService;

        public MaterialReservationRequestedConsumer(
            InventoryDbContext context,
            ILogger<MaterialReservationRequestedConsumer> logger,
            IPublishEndpoint publishEndpoint,
            IRawMaterialRepository rawMaterialRepo,          
            IStockMovementService stockMovementService
        )
        {
            _context = context;
            _logger = logger;
            _publishEndpoint = publishEndpoint;
            _rawMaterialRepo = rawMaterialRepo;
            _stockMovementService = stockMovementService;
        }

        // ─── STEP 3: Consume method ───
    public async Task Consume(ConsumeContext<MaterialReservationRequestedEvent> context)
    {
        var @event = context.Message;
        _logger.LogInformation(
            "Received reservation — PO: {OrderNumber}, WO: {WONumber}",
            @event.ProductionOrderNumber, @event.WorkOrderNumber ?? "N/A (PO-level)");
        
        // ─── IDEMPOTENCY CHECK (WorkOrderId-based, clean!) ───
        if (@event.WorkOrderId.HasValue)
        {
            // WO-level: exact WorkOrderId match
            var woReservedCount = await _context.StockMovements
                .CountAsync(sm => sm.WorkOrderId == @event.WorkOrderId
                            && sm.MovementType == MovementType.RESERVE);
            if (woReservedCount >= @event.Materials.Count)
            {
                _logger.LogWarning("All materials already reserved for WO {WONumber} — skipping",
                    @event.WorkOrderNumber);
                
                await _publishEndpoint.Publish(new StockReservedEvent
                {
                    ProductionOrderId = @event.ProductionOrderId,
                    WorkOrderId = @event.WorkOrderId,
                    Success = true,
                    FailureReason = null,
                    ReservedMaterials = @event.Materials.Select(m => new ReservedMaterial
                    {
                        RawMaterialId = m.RawMaterialId,
                        QuantityReserved = m.Quantity
                    }).ToList()
                });
                return;
            }
        }
        else
        {
            // PO-level: no WO, check by PO + null WorkOrderId
            var poReservedCount = await _context.StockMovements
                .CountAsync(sm => sm.ReferenceType == ReferenceType.PRODUCTION_ORDER
                            && sm.ReferenceId == @event.ProductionOrderId
                            && sm.WorkOrderId == null
                            && sm.MovementType == MovementType.RESERVE);
            if (poReservedCount >= @event.Materials.Count)
            {
                _logger.LogWarning("All {Count} materials already reserved for PO {OrderNumber} — skipping",
                    poReservedCount, @event.ProductionOrderNumber);

                await _publishEndpoint.Publish(new StockReservedEvent
                {
                    ProductionOrderId = @event.ProductionOrderId,
                    WorkOrderId = @event.WorkOrderId,
                    Success = true,
                    FailureReason = null,
                    ReservedMaterials = @event.Materials.Select(m => new ReservedMaterial
                    {
                        RawMaterialId = m.RawMaterialId,
                        QuantityReserved = m.Quantity
                    }).ToList()
                });
                return;
            }
        }
    
        var reservedMaterials = new List<ReservedMaterial>();
        bool allSuccess = true;
        string? failReason = null;

        try
        {
            // ─── STEP 4: Har material check karo ───
            foreach (var material in @event.Materials)
            {
                // A: Get inventories across all warehouses
                var inventories = await _rawMaterialRepo.GetInventoriesAsync(material.RawMaterialId);
                var totalAvailable = inventories.Sum(i => i.AvailableStock);

                // B: Check — kya enough stock hai?
                if (totalAvailable < material.Quantity)
                {
                    allSuccess = false;
                    failReason = $"Insufficient stock for {material.MaterialCode}: " +
                                 $"need {material.Quantity}, available {totalAvailable}";
                    break;
                }

                // C: Waterfall Allocation — split across locations if needed
                var remaining = material.Quantity;
                var sortedInventories = inventories
                    .Where(i => i.AvailableStock > 0)
                    .OrderByDescending(i => i.AvailableStock)
                    .ToList();

                foreach (var inv in sortedInventories)
                {
                    if (remaining <= 0) break;

                    var allocate = Math.Min(remaining, inv.AvailableStock);

                    var notes = @event.WorkOrderId.HasValue
                        ? $"Reserved for {@event.ProductionOrderNumber} / WO: {@event.WorkOrderNumber}"
                        : $"Reserved for {@event.ProductionOrderNumber}";

                    if (!string.IsNullOrEmpty(@event.BomCode))
                    {
                        notes += $" (BOM: {@event.BomCode} v{@event.BomVersion})";
                    }

                    if (sortedInventories.Count > 1)
                    {
                        notes += $" [Split: {allocate} of {material.Quantity}]";
                    }

                    await _stockMovementService.RecordMovementAsync(new RecordStockMovementDto
                    {
                        MovementType  = MovementType.RESERVE,
                        ItemType      = ItemType.RAW_MATERIAL,
                        ItemId        = material.RawMaterialId,
                        StorageLocationId   = inv.StorageLocationId,
                        Quantity      = allocate,
                        ReferenceType = ReferenceType.PRODUCTION_ORDER,
                        ReferenceId   = @event.ProductionOrderId,
                        WorkOrderId   = @event.WorkOrderId,
                        Notes         = notes,
                        CreatedAt     = DateTime.UtcNow,
                        CreatedBy     = SystemUser.Id
                    });

                    remaining -= allocate;

                    _logger.LogInformation(
                        "Reserved {Qty} of {MaterialCode} from Location {LocId} (remaining: {Remaining})",
                        allocate, material.MaterialCode, inv.StorageLocationId, remaining);
                }

                // D: Track reserved
                reservedMaterials.Add(new ReservedMaterial
                {
                    RawMaterialId = material.RawMaterialId,
                    QuantityReserved = material.Quantity
                });
            }
        }
        catch (Exception ex)
        {
            // Gracefully handle — don't let exceptions crash the consumer
            allSuccess = false;
            failReason = ex.Message;
            _logger.LogError(ex, "Reservation failed for {Target}: {Error}",
                @event.WorkOrderId.HasValue ? @event.WorkOrderNumber : @event.ProductionOrderNumber,
                ex.Message);
        }

        // ─── STEP 5: Response — ALWAYS publish result (success or failure) ───
        await _publishEndpoint.Publish(new StockReservedEvent
        {
            ProductionOrderId = @event.ProductionOrderId,
            WorkOrderId = @event.WorkOrderId,       // Forward WO id back
            Success = allSuccess,
            FailureReason = failReason,
            ReservedMaterials = reservedMaterials
        });

        _logger.LogInformation("Reservation {Result} for {Target}: {OrderNumber}",
            allSuccess ? "SUCCESS" : "FAILED",
            @event.WorkOrderId.HasValue ? "WO" : "PO",
            @event.WorkOrderId.HasValue ? @event.WorkOrderNumber : @event.ProductionOrderNumber);
    }
}