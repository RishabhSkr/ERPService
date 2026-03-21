using MyERP.Services.Production.Constants;

namespace MyERP.Services.Production.Models
{
    /// <summary>
    /// Equipment — machine/tool inside a WorkCenter. One equipment can handle multiple processes.
    /// </summary>
    public class Equipment
    {
        public Guid EquipmentId { get; set; }
        public string EquipmentCode { get; set; } = string.Empty;  // "EQ-CUT-001"
        public string EquipmentName { get; set; } = string.Empty;  // "CNC Cutting Machine"

        // Belongs to which WorkCenter
        public Guid WorkCenterId { get; set; }
        public virtual WorkCenter? WorkCenter { get; set; }

        public string? Manufacturer { get; set; }
        public string? Model { get; set; }
        public string Status { get; set; } = EquipmentStatus.Active;  // Active, Maintenance, Retired
        public decimal CostPerHour { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
