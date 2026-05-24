using MyERP.Services.Production.Constants;

namespace MyERP.Services.Production.Models
{
    /// <summary>
    /// WorkOrder — runtime task on shop floor.
    /// User creates manually from PO with specific qty.
    /// Equipment assigned at activation via WorkOrderExecution.
    /// 
    /// State: Pending → Released → InProgress → Completed / Cancelled
    /// </summary>
    public class WorkOrder
    {
        public Guid WorkOrderId { get; set; }
        public string WorkOrderNumber { get; set; } = string.Empty;  // "WO-2026-0001"

        // ====================================
        // PARENT LINK
        // ====================================
        public Guid ProductionOrderId { get; set; }
        public virtual ProductionOrder? ProductionOrder { get; set; }

        // ====================================
        // FROM PROCESS ROUTE (auto-populated)
        // ====================================
        public Guid? ProcessRouteStepId { get; set; }
        public virtual ProcessRouteStep? ProcessRouteStep { get; set; }

        public Guid? ProcessId { get; set; }
        public virtual Process? Process { get; set; }

        public int StepNumber { get; set; }
        public string OperationName { get; set; } = string.Empty;

        // Route version lock (like BomCode + BomVersion on PO)
        public string? RouteCode { get; set; }
        public int? RouteVersion { get; set; }

        // ====================================
        // FROM PO (auto-populated)
        // ====================================
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;

        // ====================================
        // USER SPECIFIES AT CREATION
        // ====================================
        public Guid? WorkCenterId { get; set; }         // user picks work center
        public virtual WorkCenter? WorkCenter { get; set; }

        public decimal QuantityPlanned { get; set; }

        // Scheduling (user optional)
        public DateTime? ScheduledStart { get; set; }
        public DateTime? ScheduledEnd { get; set; }
        public string? Notes { get; set; }

        // ====================================
        // RUNTIME TRACKING
        // ====================================
        public decimal QuantityCompleted { get; set; }
        public decimal QuantityScrap { get; set; }

        // Status: Pending → Released → InProgress → Completed / Cancelled
        public string Status { get; set; } = WorkOrderStatus.Pending;

        // Actual dates (set by system)
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualEndDate { get; set; }

        // ====================================
        // WO-LEVEL RESERVATION (same pattern as PO)
        // ====================================
        public string? ReservationStatus { get; set; }       // Pending, Reserved, Failed
        public string? ReservationFailReason { get; set; }
        public int ReservationAttempts { get; set; } = 0;

        // ====================================
        // CANCEL
        // ====================================
        public string? CancelReason { get; set; }

        // ====================================
        // AUDIT
        // ====================================
        public Guid? CreatedBy { get; set; }
        public Guid? UpdatedBy { get; set; }
        public Guid? StartedBy { get; set; }
        public Guid? CompletedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation — execution history on equipment
        public virtual ICollection<WorkOrderExecution> Executions { get; set; } = new List<WorkOrderExecution>();
    }
}
