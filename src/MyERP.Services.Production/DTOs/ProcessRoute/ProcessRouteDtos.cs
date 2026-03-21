namespace MyERP.Services.Production.DTOs.ProcessRoute
{
    public class CreateProcessRouteDto
    {
        public string RouteCode { get; set; } = string.Empty;
        public Guid ProductId { get; set; }
        public Guid WorkCenterId { get; set; }
        public string? Description { get; set; }
        public List<CreateProcessRouteStepDto> Steps { get; set; } = new();
    }

    public class CreateProcessRouteStepDto
    {
        public int StepNumber { get; set; }
        public Guid ProcessId { get; set; }
        public Guid? EquipmentId { get; set; }
        public int SetupTimeMinutes { get; set; }
        public int RunTimePerUnitMinutes { get; set; }
        public string? Notes { get; set; }
        public List<CreateStepMaterialDto> Materials { get; set; } = new();
    }

    public class CreateStepMaterialDto
    {
        public Guid? BOMLineId { get; set; }
        public Guid RawMaterialId { get; set; }
        public string MaterialCode { get; set; } = string.Empty;
        public string MaterialName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
    }

    public class ProcessRouteDto
    {
        public Guid ProcessRouteId { get; set; }
        public string RouteCode { get; set; } = string.Empty;
        public Guid ProductId { get; set; }
        public string WorkCenterCode { get; set; } = string.Empty;
        public string WorkCenterName { get; set; } = string.Empty;
        public int Version { get; set; }
        public bool IsActive { get; set; }
        public string? Description { get; set; }
        public List<ProcessRouteStepDto> Steps { get; set; } = new();
    }

    public class ProcessRouteStepDto
    {
        public Guid ProcessRouteStepId { get; set; }
        public int StepNumber { get; set; }
        public string ProcessCode { get; set; } = string.Empty;
        public string ProcessName { get; set; } = string.Empty;
        public string? EquipmentCode { get; set; }
        public int SetupTimeMinutes { get; set; }
        public int RunTimePerUnitMinutes { get; set; }
        public string? Notes { get; set; }
        public List<StepMaterialDto> Materials { get; set; } = new();
    }

    public class StepMaterialDto
    {
        public Guid RawMaterialId { get; set; }
        public string MaterialCode { get; set; } = string.Empty;
        public string MaterialName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
    }
}
