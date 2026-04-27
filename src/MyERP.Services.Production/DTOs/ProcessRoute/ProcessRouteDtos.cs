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
        // Materials removed — now defined in BOM Line (industry standard)
    }

    public class ProcessRouteDto
    {
        public Guid ProcessRouteId { get; set; }
        public string RouteCode { get; set; } = string.Empty;
        public Guid ProductId { get; set; }
        public Guid WorkCenterId { get; set; }
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
        public Guid ProcessId { get; set; }
        public string ProcessCode { get; set; } = string.Empty;
        public string ProcessName { get; set; } = string.Empty;
        public Guid? EquipmentId { get; set; }
        public string? EquipmentCode { get; set; }
        public string? EquipmentName { get; set; }
        public int SetupTimeMinutes { get; set; }
        public int RunTimePerUnitMinutes { get; set; }
        public string? Notes { get; set; }
        
    }
}
