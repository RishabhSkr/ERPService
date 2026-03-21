using MassTransit;
using MyERP.Shared.Events;
using MyERP.Services.Inventory.Data;
using MyERP.Services.Inventory.DTOs.StockMovements;
using MyERP.Services.Inventory.Repositories.RawMaterials;
using MyERP.Services.Inventory.Services.StockMovements;
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
        var @event = context.Message;  // Production se aaya data
        _logger.LogInformation("Received reservation for PO: {OrderNumber}", @event.ProductionOrderNumber);
        
        // ─── NEW: Check duplicate reservation ───
        // var alreadyProcessed = await _context.StockMovements
        // .AnyAsync(sm => sm.ReferenceType == "ProductionOrder" 
        //              && sm.ReferenceId == @event.ProductionOrderId
        //              && sm.MovementType == "RESERVE");

        // idempotency check:
        // Count how many materials ALREADY reserved for this PO
        var reservedCount = await _context.StockMovements
            .CountAsync(sm => sm.ReferenceType == "ProductionOrder" 
                        && sm.ReferenceId == @event.ProductionOrderId
                        && sm.MovementType == "RESERVE");
        // Total materials needed
        var totalMaterials = @event.Materials.Count;
        // If ALL materials already reserved → duplicate, skip!
        // If PARTIAL or ZERO → allow retry
        if (reservedCount >= totalMaterials)
        {
            _logger.LogWarning("All {Count} materials already reserved for PO {OrderNumber} — skipping", 
                reservedCount, @event.ProductionOrderNumber);
            return;
        }
    
        var reservedMaterials = new List<ReservedMaterial>();  // Track karo kya reserve hua
        bool allSuccess = true;
        string? failReason = null;
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
                break;  // Ek bhi fail → sab fail
            }
            // C: Reserve — existing StockMovementService use karo!
            var firstInventory = inventories.First(i => i.AvailableStock > 0);
            await _stockMovementService.RecordMovementAsync(new RecordStockMovementDto
            {
                MovementType  = "RESERVE",
                ItemType      = "RawMaterial",
                ItemId        = material.RawMaterialId,
                WarehouseId   = firstInventory.WarehouseId,
                Quantity      = material.Quantity,
                ReferenceType = "ProductionOrder",             
                ReferenceId   = @event.ProductionOrderId,      // TRACEABILITY
                Notes         = $"Reserved for {@event.ProductionOrderNumber}"
            });
            // D: Track reserved
            reservedMaterials.Add(new ReservedMaterial    // NOT StockReservedEvent!
            {
                RawMaterialId = material.RawMaterialId,
                QuantityReserved = material.Quantity
            });
        }

        // ─── STEP 5: Response wapas bhejo Production ko ───
        await _publishEndpoint.Publish(new StockReservedEvent
        {
            ProductionOrderId = @event.ProductionOrderId,
            Success = allSuccess,
            FailureReason = failReason,
            ReservedMaterials = reservedMaterials
        });
        _logger.LogInformation("Reservation {Result} for PO: {OrderNumber}",
            allSuccess ? "SUCCESS" : "FAILED", @event.ProductionOrderNumber);
    }
}