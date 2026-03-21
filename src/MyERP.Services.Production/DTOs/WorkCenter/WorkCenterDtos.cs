namespace MyERP.Services.Production.DTOs.WorkCenter
{
    public class CreateWorkCenterDto
    {
        public string CenterCode { get; set; } = string.Empty;
        public string CenterName { get; set; } = string.Empty;
        public string? Location { get; set; }
        public decimal CostPerHour { get; set; }
        public int CapacityPerHour { get; set; }
        public string? Description { get; set; }
    }

    public class WorkCenterDto
    {
        public Guid WorkCenterId { get; set; }
        public string CenterCode { get; set; } = string.Empty;
        public string CenterName { get; set; } = string.Empty;
        public string? Location { get; set; }
        public decimal CostPerHour { get; set; }
        public int CapacityPerHour { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int EquipmentCount { get; set; }
    }
}
