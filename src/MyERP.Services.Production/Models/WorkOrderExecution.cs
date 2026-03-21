using MyERP.Services.Production.Constants;

namespace MyERP.Services.Production.Models
{
    /// <summary>
    /// WorkOrderExecution — tracks WO activation on equipment.
    /// One WO can run on multiple equipment simultaneously.
    /// Provides full traceability: who activated, when, on which machine, how much produced.
    /// </summary>
    public class WorkOrderExecution
    {
        public Guid ExecutionId { get; set; }

        // Which work order
        public Guid WorkOrderId { get; set; }
        public virtual WorkOrder? WorkOrder { get; set; }

        // Which equipment
        public Guid EquipmentId { get; set; }
        public virtual Equipment? Equipment { get; set; }

        // Who & When
        public string ActivatedBy { get; set; } = string.Empty;
        public DateTime ActivatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DeactivatedAt { get; set; }

        // Output from this equipment
        public decimal QuantityProduced { get; set; }
        public decimal QuantityScrap { get; set; }

        // Status: Active → Paused → Completed
        public string Status { get; set; } = ExecutionStatus.Active;

        public string? Notes { get; set; }
    }
}
