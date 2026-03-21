using MyERP.Services.Production.Constants;

namespace MyERP.Services.Production.Models
{
    /// <summary>
    /// WorkOrder — runtime task on shop floor, generated from PO + ProcessRoute.
    /// NO EquipmentId — equipment assigned at activation via WorkOrderExecution.
    /// </summary>
    public class WorkOrder
    {
        public Guid WorkOrderId { get; set; }
        public string WorkOrderNumber { get; set; } = string.Empty;  // "WO-2026-0001"

        // Link to Production Order (inherits BOM, Quantity)
        public Guid ProductionOrderId { get; set; }
        public virtual ProductionOrder? ProductionOrder { get; set; }

        // Link to ProcessRoute step
        public Guid? ProcessRouteStepId { get; set; }
        public virtual ProcessRouteStep? ProcessRouteStep { get; set; }

        // Denormalized for quick access
        public Guid? ProcessId { get; set; }
        public virtual Process? Process { get; set; }

        public Guid? WorkCenterId { get; set; }
        public virtual WorkCenter? WorkCenter { get; set; }

        public int StepNumber { get; set; }
        public string OperationName { get; set; } = string.Empty;

        // Quantities (aggregated from executions)
        public decimal QuantityPlanned { get; set; }
        public decimal QuantityCompleted { get; set; }
        public decimal QuantityScrap { get; set; }

        // Status: Pending → InProgress → Completed → Cancelled
        public string Status { get; set; } = WorkOrderStatus.Pending;

        // Scheduling
        public DateTime? ScheduledStart { get; set; }
        public DateTime? ScheduledEnd { get; set; }

        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation — execution history on equipment
        public virtual ICollection<WorkOrderExecution> Executions { get; set; } = new List<WorkOrderExecution>();
    }
}
