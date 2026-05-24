/*
 * Shared Event Contracts
 * 
 * 📚 INDUSTRY PRACTICE: Cross-Service Events
 * 
 * ❌ NOOB: Duplicate event classes in each service with different namespaces
 *    Result: MassTransit can't route messages between services
 * 
 * ✅ INDUSTRY: Shared contracts library referenced by all services
 *    - Same type name used for publish AND consume
 *    - MassTransit uses fully qualified type name for routing
 *    - All services reference this shared library
 */

namespace MyERP.Shared.Events
{
    // ========================================
    // SALES EVENTS
    // ========================================
    
    /// <summary>
    /// Published by Sales when a new order is created.
    /// Consumed by Production to create PendingRequest.
    /// </summary>
    public class SalesOrderCreatedEvent
    {
        public Guid EventId { get; set; } = Guid.NewGuid();
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
        public string EventType { get; set; } = "SalesOrderCreated";
        
        public Guid SalesOrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public List<SalesOrderItemEvent> Items { get; set; } = new();
    }

    public class SalesOrderItemEvent
    {
        public Guid ProductId { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    /// <summary>
    /// Published by Sales when an order is cancelled.
    /// </summary>
    public class SalesOrderCancelledEvent
    {
        public Guid EventId { get; set; } = Guid.NewGuid();
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
        public string EventType { get; set; } = "SalesOrderCancelled";
        
        public Guid SalesOrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }

    // ========================================
    // PRODUCTION EVENTS
    // ========================================
    
    /// <summary>
    /// Published by Production when materials need to be reserved.
    /// Consumed by Inventory.
    /// </summary>
    public class MaterialReservationRequestedEvent
    {
        public Guid EventId { get; set; } = Guid.NewGuid();
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
        public string EventType { get; set; } = "MaterialReservationRequested";
        
        public Guid ProductionOrderId { get; set; }
        public string ProductionOrderNumber { get; set; } = string.Empty;

        // WO-level reservation (nullable = backward compatible with PO-level)
        public Guid? WorkOrderId { get; set; }
        public string? WorkOrderNumber { get; set; }

        public string? BomCode { get; set; }
        public int? BomVersion { get; set; }
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
    /// Published by Production when order is cancelled.
    /// Consumed by Inventory to release reservations.
    /// </summary>
    public class ProductionOrderCancelledEvent
    {
        public Guid EventId { get; set; } = Guid.NewGuid();
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
        public string EventType { get; set; } = "ProductionOrderCancelled";
        
        public Guid? SalesOrderId{get; set;}
        public string? SalesOrderNumber {get;set;}
        
        public Guid ProductionOrderId { get; set; }
        public string ProductionOrderNumber { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Published by Production when batch is complete.
    /// Consumed by Inventory to add finished goods.
    /// </summary>
    public class BatchConcludedEvent
    {
        public Guid EventId { get; set; } = Guid.NewGuid();
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
        public string EventType { get; set; } = "BatchConcluded";
        
        public Guid ProductionOrderId { get; set; }
        public string ProductionOrderNumber { get; set; } = string.Empty;

        public Guid? SalesOrderId { get; set; }        
        public string? SalesOrderNumber { get; set; }

        // WO-level completion (nullable = backward compatible with PO-level)
        public Guid? WorkOrderId { get; set; }
        public string? WorkOrderNumber { get; set; }

        public Guid ProductId { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public decimal QuantityGood { get; set; }
        public decimal QuantityScrap { get; set; }
        public List<MaterialConsumed> MaterialsConsumed { get; set; } = new();
    }

    public class MaterialConsumed
    {
        public Guid RawMaterialId { get; set; }
        public string MaterialCode { get; set; } = string.Empty;
        public decimal QuantityConsumed { get; set; }
        public decimal QuantityReturned { get; set; }
        public string Unit { get; set; } = string.Empty;
    }

    /// <summary>
    /// Published by Production when cancelled "&" materials need returning (SAGA).
    /// Consumed by Inventory to release reservations.
    /// </summary>
    public class MaterialReturnRequestedEvent
    {
        public Guid EventId { get; set; } = Guid.NewGuid();
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
        public string EventType { get; set; } = "MaterialReturnRequested";
        
        // WO-level return (nullable = backward compatible with PO-level)
        public Guid? WorkOrderId { get; set; }
        public string? WorkOrderNumber { get; set; }

        public Guid ProductionOrderId { get; set; }
        public string ProductionOrderNumber { get; set; } = string.Empty;
        public List<MaterialConsumed> MaterialsConsumed { get; set; } = new();
    }

    // ========================================
    // INVENTORY EVENTS
    // ========================================
    
    /// <summary>
    /// Published by Inventory when stock is reserved.
    /// Consumed by Production to update order status.
    /// </summary>
    public class StockReservedEvent
    {
        public Guid EventId { get; set; } = Guid.NewGuid();
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
        
        public Guid ProductionOrderId { get; set; }

        // WO-level reservation response (nullable = backward compatible)
        public Guid? WorkOrderId { get; set; }

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
