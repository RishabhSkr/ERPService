using MassTransit;
using Microsoft.EntityFrameworkCore;
using MyERP.Services.Production.Data;
using MyERP.Shared.Events;

namespace MyERP.Services.Production.Events.Consumers;

/// <summary>
/// SAGA Compensating Action: Inventory ne materials release kiye →
/// Production me MaterialRequirements.QuantityReserved ghataao
/// </summary>
public class StockReleasedConsumer : IConsumer<StockReleasedEvent>
{
    private readonly ProductionDbContext _context;
    private readonly ILogger<StockReleasedConsumer> _logger;

    public StockReleasedConsumer(
        ProductionDbContext context,
        ILogger<StockReleasedConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<StockReleasedEvent> context)
    {
        var @event = context.Message;

        _logger.LogInformation(
            "StockReleasedEvent received — PO: {PONumber}, WO: {WONumber}, {Count} materials",
            @event.ProductionOrderNumber, @event.WorkOrderNumber ?? "N/A", @event.ReleasedMaterials.Count);

        if (!@event.ReleasedMaterials.Any()) return;

        var po = await _context.ProductionOrders
            .Include(o => o.MaterialRequirements)
            .FirstOrDefaultAsync(o => o.Id == @event.ProductionOrderId);

        if (po == null)
        {
            _logger.LogWarning("PO not found: {POId}", @event.ProductionOrderId);
            return;
        }

        foreach (var released in @event.ReleasedMaterials)
        {
            var matReq = po.MaterialRequirements
                .FirstOrDefault(r => r.RawMaterialId == released.RawMaterialId);

            if (matReq == null) continue;

            var before = matReq.QuantityReserved;
            matReq.QuantityReserved = Math.Max(0, matReq.QuantityReserved - released.QuantityReleased);

            // Update status based on new reserved qty
            matReq.Status = matReq.QuantityReserved <= 0
                ? "Pending"
                : matReq.QuantityReserved >= matReq.QuantityRequired
                    ? "Reserved"
                    : "PartiallyReserved";

            _logger.LogInformation(
                "Material {MatId}: QuantityReserved {Before} → {After} (released {Released}), Status={Status}",
                released.RawMaterialId, before, matReq.QuantityReserved, released.QuantityReleased, matReq.Status);
        }

        po.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "PO {PONumber} MaterialRequirements updated after WO {WONumber} cancellation",
            @event.ProductionOrderNumber, @event.WorkOrderNumber ?? "N/A");
    }
}
