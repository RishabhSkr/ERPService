namespace MyERP.Services.Production.DTOs.WorkOrder
{
    public class WorkOrderDto
    {
        public Guid WorkOrderId { get; set; }
        public string WorkOrderNumber { get; set; } = string.Empty;
        public string ProductionOrderNumber { get; set; } = string.Empty;
        public int StepNumber { get; set; }
        public string OperationName { get; set; } = string.Empty;
        public string? ProcessCode { get; set; }
        public string? WorkCenterCode { get; set; }
        public string? WorkCenterName { get; set; }
        public decimal QuantityPlanned { get; set; }
        public decimal QuantityCompleted { get; set; }
        public decimal QuantityScrap { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<WorkOrderExecutionDto> Executions { get; set; } = new();
    }

    public class ActivateWorkOrderDto
    {
        public Guid EquipmentId { get; set; }
        public string ActivatedBy { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class CompleteExecutionDto
    {
        public decimal QuantityProduced { get; set; }
        public decimal QuantityScrap { get; set; }
        public string? Notes { get; set; }
    }

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
}
