using MyERP.Services.Production.DTOs.Process;

namespace MyERP.Services.Production.DTOs.Equipment
{
    public class CreateEquipmentDto
    {
        public string EquipmentCode { get; set; } = string.Empty;
        public string EquipmentName { get; set; } = string.Empty;
        public Guid WorkCenterId { get; set; }
        public string? Manufacturer { get; set; }
        public string? Model { get; set; }
        public decimal CostPerHour { get; set; }
    }

    public class EquipmentDto
    {
        public Guid EquipmentId { get; set; }
        public string EquipmentCode { get; set; } = string.Empty;
        public string EquipmentName { get; set; } = string.Empty;
        public Guid WorkCenterId { get; set; }
        public string WorkCenterCode { get; set; } = string.Empty;
        public string WorkCenterName { get; set; } = string.Empty;
        public string? Manufacturer { get; set; }
        public string? Model { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal CostPerHour { get; set; }
        public bool IsActive { get; set; }
        public List<ProcessDto> LinkedProcesses { get; set; } = new();
    }

    public class LinkProcessesDto
    {
        public List<Guid> ProcessIds { get; set; } = new();
    }
}
