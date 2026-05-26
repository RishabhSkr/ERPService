/*
 * Production Events - PRODUCTION-ONLY events
 * 
 * ðŸ“š INDUSTRY PRACTICE:
 * 
 * âœ… Cross-service events (MaterialReservationRequested, StockReserved, etc.)
 *    live in MyERP.Shared.Events â€” BOTH publisher and consumer use SAME class
 *    
 * âœ… Production-internal events (BatchProgress â€” not consumed by other services)
 *    live HERE in this file
 * 
 * ðŸ“ WHY separate?
 *    MassTransit uses "Namespace:ClassName" for exchange routing
 *    If Publisher uses MyERP.Services.Production.Events.MaterialReservationRequestedEvent
 *    But Consumer uses MyERP.Shared.Events.MaterialReservationRequestedEvent
 *    â†’ Different exchanges â†’ Messages DON'T connect! ðŸ’€
 */

namespace MyERP.Services.Production.Events
{
    // ========================================
    // BASE EVENT (Production-specific)
    // ========================================
    
    /// <summary>
    /// Base event with standard metadata
    /// ðŸ“ Industry Practice: All events inherit from base
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
    /// NOT consumed by other services â€” Production internal only
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
    // CROSS-SERVICE EVENTS â†’ USE MyERP.Shared.Events
    // ========================================
    //
    // âœ… MaterialReservationRequestedEvent  â†’ MyERP.Shared.Events
    // âœ… MaterialToReserve                  â†’ MyERP.Shared.Events
    // âœ… ProductionOrderCancelledEvent      â†’ MyERP.Shared.Events
    // âœ… BatchConcludedEvent                â†’ MyERP.Shared.Events
    // âœ… MaterialConsumed                   â†’ MyERP.Shared.Events
    // âœ… MaterialReturnRequestedEvent       â†’ MyERP.Shared.Events
    // âœ… StockReservedEvent                 â†’ MyERP.Shared.Events
    // âœ… ReservedMaterial                   â†’ MyERP.Shared.Events
    //
    // Add: using MyERP.Shared.Events; wherever these are used
}
