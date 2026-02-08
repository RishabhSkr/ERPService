/*
 * Production Events - Messages published by Production Service
 * 
 * 📚 INDUSTRY vs NOOB:
 * 
 * ❌ NOOB: Put event classes everywhere, no standard structure
 * ✅ INDUSTRY:
 *    1. All events have common base fields (EventId, OccurredAt, EventType)
 *    2. Events are immutable (no setters after creation in real apps)
 *    3. Clear naming: VerbNoun pattern (MaterialReservationRequested)
 *    4. Events describe WHAT HAPPENED, not commands
 */

namespace MyERP.Services.Production.Events
{
    // ========================================
    // BASE EVENT
    // ========================================
    
    /// <summary>
    /// Base event with standard metadata
    /// 📝 Industry Practice: All events inherit from base
    ///    Provides consistency, easier logging, tracing
    /// </summary>
    public abstract class ProductionEventBase
    {
        public Guid EventId { get; set; } = Guid.NewGuid();
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
        public abstract string EventType { get; }
    }

    // ========================================
    // OUTGOING EVENTS (Published by Production)
    // ========================================

    /// <summary>
    /// Request Inventory to reserve raw materials
    /// Sent when: ProductionOrder is approved/released
    /// </summary>
    public class MaterialReservationRequestedEvent : ProductionEventBase
    {
        public override string EventType => "MaterialReservationRequested";
        
        public Guid ProductionOrderId { get; set; }
        public string ProductionOrderNumber { get; set; } = string.Empty;
        
        public List<MaterialToReserve> Materials { get; set; } = new();
    }

    public class MaterialToReserve
    {
        public Guid RawMaterialId { get; set; }
        public string MaterialCode { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
    }

    /// <summary>
    /// Notify that production order was cancelled
    /// Inventory should release any reservations
    /// </summary>
    public class ProductionOrderCancelledEvent : ProductionEventBase
    {
        public override string EventType => "ProductionOrderCancelled";
        
        public Guid ProductionOrderId { get; set; }
        public string ProductionOrderNumber { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }

    /// <summary>
    /// Real-time progress update (for dashboard)
    /// </summary>
    public class BatchProgressEvent : ProductionEventBase
    {
        public override string EventType => "BatchProgress";
        
        public Guid ProductionOrderId { get; set; }
        public string ProductionOrderNumber { get; set; } = string.Empty;
        public decimal QuantityCompleted { get; set; }
        public decimal QuantityPlanned { get; set; }
        public decimal PercentComplete => QuantityPlanned > 0 
            ? Math.Round((QuantityCompleted / QuantityPlanned) * 100, 2) 
            : 0;
    }

    /// <summary>
    /// Production batch is complete
    /// Inventory should: add finished goods, handle scrap, return unused materials
    /// </summary>
    public class BatchConcludedEvent : ProductionEventBase
    {
        public override string EventType => "BatchConcluded";
        
        public Guid ProductionOrderId { get; set; }
        public string ProductionOrderNumber { get; set; } = string.Empty;
        
        public Guid ProductId { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        
        /// <summary>
        /// Good finished products to add to inventory
        /// </summary>
        public decimal QuantityGood { get; set; }
        
        /// <summary>
        /// Scrapped units
        /// </summary>
        public decimal QuantityScrap { get; set; }
        
        /// <summary>
        /// Materials consumed (actual usage)
        /// </summary>
        public List<MaterialConsumed> MaterialsConsumed { get; set; } = new();
    }

    public class MaterialConsumed
    {
        public Guid RawMaterialId { get; set; }
        public string MaterialCode { get; set; } = string.Empty;
        public decimal QuantityConsumed { get; set; }
        public decimal QuantityReturned { get; set; }  // Unused material
        public string Unit { get; set; } = string.Empty;
    }

    // ========================================
    // INCOMING EVENTS (Consumed by Production)
    // ========================================
    
    // ❌ REMOVED: SalesOrderCreatedEvent was here causing DUPLICATE!
    // ✅ FIX: Use MyERP.Shared.Events.SalesOrderCreatedEvent instead
    //    The consumer already imports it via: using MyERP.Shared.Events;
    //
    // 📝 INDUSTRY LESSON:
    //    - Event classes must be SAME class (same namespace) for publisher and consumer
    //    - MassTransit uses "Namespace:ClassName" to create exchange names
    //    - Different namespaces = Different exchanges = Messages don't connect!

    /// <summary>
    /// Inventory confirmed stock was reserved
    /// </summary>
    public class StockReservedEvent
    {
        public Guid EventId { get; set; }
        public DateTime OccurredAt { get; set; }
        
        public Guid ProductionOrderId { get; set; }
        public bool Success { get; set; }
        public string? FailureReason { get; set; }
        public List<ReservedMaterial> ReservedMaterials { get; set; } = new();
    }

    public class ReservedMaterial
    {
        public Guid RawMaterialId { get; set; }
        public decimal QuantityReserved { get; set; }
    }
}
