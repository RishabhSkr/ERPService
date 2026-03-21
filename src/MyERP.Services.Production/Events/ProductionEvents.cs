/*
 * Production Events - PRODUCTION-ONLY events
 * 
 * 📚 INDUSTRY PRACTICE:
 * 
 * ✅ Cross-service events (MaterialReservationRequested, StockReserved, etc.)
 *    live in MyERP.Shared.Events — BOTH publisher and consumer use SAME class
 *    
 * ✅ Production-internal events (BatchProgress — not consumed by other services)
 *    live HERE in this file
 * 
 * 📝 WHY separate?
 *    MassTransit uses "Namespace:ClassName" for exchange routing
 *    If Publisher uses MyERP.Services.Production.Events.MaterialReservationRequestedEvent
 *    But Consumer uses MyERP.Shared.Events.MaterialReservationRequestedEvent
 *    → Different exchanges → Messages DON'T connect! 💀
 */

namespace MyERP.Services.Production.Events
{
    // ========================================
    // BASE EVENT (Production-specific)
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
    // PRODUCTION-ONLY EVENTS (NOT cross-service)
    // ========================================

    /// <summary>
    /// Real-time progress update (for dashboard)
    /// NOT consumed by other services — Production internal only
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

    // ========================================
    // CROSS-SERVICE EVENTS → USE MyERP.Shared.Events
    // ========================================
    //
    // ✅ MaterialReservationRequestedEvent  → MyERP.Shared.Events
    // ✅ MaterialToReserve                  → MyERP.Shared.Events
    // ✅ ProductionOrderCancelledEvent      → MyERP.Shared.Events
    // ✅ BatchConcludedEvent                → MyERP.Shared.Events
    // ✅ MaterialConsumed                   → MyERP.Shared.Events
    // ✅ MaterialReturnRequestedEvent       → MyERP.Shared.Events
    // ✅ StockReservedEvent                 → MyERP.Shared.Events
    // ✅ ReservedMaterial                   → MyERP.Shared.Events
    //
    // Add: using MyERP.Shared.Events; wherever these are used
}
