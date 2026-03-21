/*
 * ProductionOrder - The actual manufacturing instruction
 * 
 * 📚 INDUSTRY vs NOOB:
 * 
 * ❌ NOOB: One status field, all data in one table
 * ✅ INDUSTRY:
 *    1. Lifecycle states with clear transitions
 *    2. Planned vs Actual quantities (track variance)
 *    3. Planned vs Actual dates (track delays)
 *    4. Good quantity vs Scrap quantity (quality tracking)
 *    5. Link back to SalesOrder for traceability
 */

using MyERP.Services.Production.Constants;

namespace MyERP.Services.Production.Models
{
    /// <summary>
    /// Production Order - Instruction to manufacture products
    /// Created when PendingRequest is approved
    /// </summary>
    public class ProductionOrder
    {
        public Guid Id { get; set; }
        
        /// <summary>
        /// Human-readable number: PO-2026-0001
        /// 📝 Industry: Sequential, year-prefixed, easy to communicate
        /// </summary>
        public string OrderNumber { get; set; } = string.Empty;
        
        /// <summary>
        /// Link to original Sales Order (traceability)
        /// </summary>
        public Guid? SalesOrderId { get; set; }
        public string? SalesOrderNumber { get; set; }
        
        /// <summary>
        /// Product to manufacture
        /// </summary>
        public Guid ProductId { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        
        /// <summary>
        /// BOM used for this order
        /// </summary>
        public Guid BOMId { get; set; }
        
        // ====================================
        // QUANTITY TRACKING
        // ====================================
        
        /// <summary>
        /// Original planned quantity
        /// </summary>
        public decimal QuantityPlanned { get; set; }
        
        /// <summary>
        /// Completed good units
        /// </summary>
        public decimal QuantityGood { get; set; } = 0;
        
        /// <summary>
        /// Scrapped/defective units
        /// </summary>
        public decimal QuantityScrap { get; set; } = 0;
        
        /// <summary>
        /// Total produced = Good + Scrap
        /// 📝 Industry: Calculated property, not stored
        /// </summary>
        public decimal QuantityProduced => QuantityGood + QuantityScrap;
        
        // ====================================
        // DATE TRACKING
        // ====================================
        
        public DateTime PlannedStartDate { get; set; }
        public DateTime PlannedEndDate { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        
        // ====================================
        // STATUS LIFECYCLE
        // ====================================
        // BOM Version Lock (from Monolith best practice)
        public string? BomCode { get; set; }
        public int? BomVersion { get; set; }

        // Reservation Sub-State
        public string? ReservationStatus { get; set; }       // null, Pending, Reserved, Failed
        public string? ReservationFailReason { get; set; }
        public int ReservationAttempts { get; set; } = 0;
        public DateTime? LastReservationAttempt { get; set; }

        // Release Audit
        public DateTime? ReleasedAt { get; set; }
        public Guid? ReleasedBy { get; set; }

        // Cancel Tracking
        public string? CancelReason { get; set; }
        public DateTime? CancelledAt { get; set; }
        public bool MaterialsReturned { get; set; } = false;

        /// <summary>
        /// Status: Create, Released, InProgress, Completed, Cancelled
        /// </summary>
        public string Status { get; set; } = ProductionOrderStatus.Create;
        
        /// <summary>
        /// Priority for scheduling: 1 (highest) to 5 (lowest)
        /// </summary>
        public int Priority { get; set; } = 3;
        
        public string? Notes { get; set; }
        
        // Audit
        public Guid? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation
        public virtual BOM? BOM { get; set; }
        public virtual ICollection<MaterialRequirement> MaterialRequirements { get; set; } = new List<MaterialRequirement>();
    }
    
    /// <summary>
    /// Material requirements calculated from BOM explosion
    /// </summary>
    public class MaterialRequirement
    {
        public Guid Id { get; set; }
        public Guid ProductionOrderId { get; set; }
        
        public Guid RawMaterialId { get; set; }
        public string MaterialCode { get; set; } = string.Empty;
        public string MaterialName { get; set; } = string.Empty;
        
        /// <summary>
        /// Required quantity = BOM quantity × Production quantity
        /// </summary>
        public decimal QuantityRequired { get; set; }
        
        /// <summary>
        /// Reserved in Inventory Service
        /// </summary>
        public decimal QuantityReserved { get; set; } = 0;
        
        /// <summary>
        /// Actually consumed during production
        /// </summary>
        public decimal QuantityConsumed { get; set; } = 0;
        
        public string Unit { get; set; } = "pcs";
        
        /// <summary>
        /// Status: Pending, Reserved, PartiallyReserved, Consumed
        /// </summary>
        public string Status { get; set; } = MaterialRequirementStatus.Pending;
        
        // Navigation
        public virtual ProductionOrder? ProductionOrder { get; set; }
    }
}
