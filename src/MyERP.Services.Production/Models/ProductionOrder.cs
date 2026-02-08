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
        
        /// <summary>
        /// Status: Draft, Released, InProgress, Completed, Cancelled
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
        public string Status { get; set; } = "Pending";
        
        // Navigation
        public virtual ProductionOrder? ProductionOrder { get; set; }
    }
    
    /// <summary>
    /// Status constants for Production Order lifecycle
    /// 
    /// Industry Practice: State Machine
    ///    Create → Released → InProgress → Completed
    ///              ↓
    ///          Cancelled (from any state except Completed)
    /// </summary>
    public static class ProductionOrderStatus
    {
        public const string Create = "Create";           // Just created, can edit
        public const string Released = "Released";     // Approved, materials reserved
        public const string InProgress = "InProgress"; // Production started
        public const string Completed = "Completed";   // Finished
        public const string Cancelled = "Cancelled";   // Cancelled
    }
}
