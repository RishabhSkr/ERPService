namespace MyERP.Services.Production.DTOs.WorkOrder
{
    // ====================================
    // CREATE WO (User input)
    // ====================================
    public class CreateWorkOrderDto
    {
        public Guid ProductionOrderId { get; set; }
        public Guid ProcessRouteStepId { get; set; }   // which step
        public Guid WorkCenterId { get; set; }          // user picks work center
        public decimal QuantityPlanned { get; set; }
        public DateTime? ScheduledStart { get; set; }
        public DateTime? ScheduledEnd { get; set; }
        public string? Notes { get; set; }
    }

    // ====================================
    // WO RESPONSE
    // ====================================
    public class WorkOrderDto
    {
        public Guid WorkOrderId { get; set; }
        public string WorkOrderNumber { get; set; } = string.Empty;
        public Guid ProductionOrderId { get; set; }
        public string ProductionOrderNumber { get; set; } = string.Empty;

        // Product info (from PO)
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;

        // Route info
        public int StepNumber { get; set; }
        public string OperationName { get; set; } = string.Empty;
        public string? ProcessCode { get; set; }
        public string? RouteCode { get; set; }
        public int? RouteVersion { get; set; }

        // Work Center
        public Guid? WorkCenterId { get; set; }
        public string? WorkCenterCode { get; set; }
        public string? WorkCenterName { get; set; }

        // Quantities
        public decimal QuantityPlanned { get; set; }
        public decimal QuantityCompleted { get; set; }
        public decimal QuantityScrap { get; set; }
        public string OutputUnit { get; set; } = "pcs";

        // Status + Reservation
        public string Status { get; set; } = string.Empty;
        public string? ReservationStatus { get; set; }
        public string? ReservationFailReason { get; set; }

        // Dates
        public DateTime? ScheduledStart { get; set; }
        public DateTime? ScheduledEnd { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualEndDate { get; set; }

        // Cancel
        public string? CancelReason { get; set; }

        // Audit
        public Guid? CreatedBy { get; set; }
        public Guid? StartedBy { get; set; }
        public Guid? CompletedBy { get; set; }
        public DateTime CreatedAt { get; set; }

        public string? Notes { get; set; }
        public List<WorkOrderExecutionDto> Executions { get; set; } = new();
    }

    // ====================================
    // ACTIVATE ON EQUIPMENT
    // ====================================
    public class ActivateWorkOrderDto
    {
        public Guid EquipmentId { get; set; }
        public string ActivatedBy { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    // ====================================
    // COMPLETE EXECUTION
    // ====================================
    public class CompleteExecutionDto
    {
        public decimal QuantityProduced { get; set; }
        public decimal QuantityScrap { get; set; }
        public string? Notes { get; set; }
    }

    // ====================================
    // CANCEL WO (with reason)
    // ====================================
    public class CancelWorkOrderDto
    {
        public string Reason { get; set; } = string.Empty;
    }

    // ====================================
    // EXECUTION RESPONSE (tracking)
    // ====================================
    public class WorkOrderExecutionDto
    {
        public Guid ExecutionId { get; set; }
        public string EquipmentCode { get; set; } = string.Empty;
        public string EquipmentName { get; set; } = string.Empty;
        public string ActivatedBy { get; set; } = string.Empty;
        public DateTime ActivatedAt { get; set; }
        public DateTime? DeactivatedAt { get; set; }
        public decimal QuantityProduced { get; set; }
        public decimal QuantityScrap { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    // ====================================
    // DASHBOARD (Per PO, per step aggregation)
    // ====================================
    public class WorkOrderDashboardDto
    {
        public Guid ProductionOrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public decimal PoQuantityPlanned { get; set; }
        public string PoStatus { get; set; } = string.Empty;
        public List<StepSummaryDto> Steps { get; set; } = new();
    }

    public class StepSummaryDto
    {
        public Guid ProcessRouteStepId { get; set; }
        public int StepNumber { get; set; }
        public string? ProcessCode { get; set; }
        public string ProcessName { get; set; } = string.Empty;
        public decimal OutputMultiplier { get; set; } = 1.0m;
        public string OutputUnit { get; set; } = "pcs";
        public decimal TargetQuantity { get; set; }     // PO Qty * Multiplier
        public decimal TotalPlanned { get; set; }       // HybridSum of WOs (in OutputUnit)
        public decimal TotalCompleted { get; set; }      // SUM completed WOs (in OutputUnit)
        public decimal UnplannedQuantity { get; set; }   // TargetQuantity - HybridSum
        public decimal ProgressPercentage { get; set; }
        public int WoCount { get; set; }
        public string DisplayStatus { get; set; } = string.Empty;
    }

    // ====================================
    // PLANNING INFO (Before Creating WO)
    // ====================================
    public class WorkOrderPlanningInfoDto
    {
        public Guid ProductionOrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public decimal PoQuantityPlanned { get; set; }
        public string? RouteCode { get; set; }
        public int? RouteVersion { get; set; }
        public List<PlanningStepDto> Steps { get; set; } = new();
    }

    public class PlanningStepDto
    {
        public Guid ProcessRouteStepId { get; set; }
        public int StepNumber { get; set; }
        public string? ProcessCode { get; set; }
        public string ProcessName { get; set; } = string.Empty;
        public decimal OutputMultiplier { get; set; } = 1.0m;
        public string OutputUnit { get; set; } = "pcs";
        public decimal TargetQuantity { get; set; }      // PO Qty * Multiplier
        public decimal RemainingQuantity { get; set; }   // TargetQuantity - HybridSum
        public int SetupTimeMinutes { get; set; }
        public int RunTimePerUnitMinutes { get; set; }

        // Equipment capable of this process in any valid work center
        public List<AvailableEquipmentDto> LinkedEquipment { get; set; } = new();

        // Existing WOs for this step
        public List<ExistingWorkOrderDto> ExistingWorkOrders { get; set; } = new();
    }

    public class AvailableEquipmentDto
    {
        public Guid EquipmentId { get; set; }
        public string EquipmentCode { get; set; } = string.Empty;
        public string EquipmentName { get; set; } = string.Empty;
        public Guid WorkCenterId { get; set; }
        public string WorkCenterCode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal CostPerHour { get; set; }
    }

    public class ExistingWorkOrderDto
    {
        public Guid WorkOrderId { get; set; }
        public string WorkOrderNumber { get; set; } = string.Empty;
        public decimal QuantityPlanned { get; set; }
        public decimal QuantityCompleted { get; set; }
        public string OutputUnit { get; set; } = "pcs";
        public string Status { get; set; } = string.Empty;
        public string? WorkCenterCode { get; set; }
    }
}
