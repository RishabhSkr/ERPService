namespace MyERP.Services.Production.Models
{
    /// <summary>
    /// WorkCenter — physical workshop/station where processes happen
    /// </summary>
    public class WorkCenter
    {
        public Guid WorkCenterId { get; set; }
        public string CenterCode { get; set; } = string.Empty;    // "WC-RAW-001"
        public string CenterName { get; set; } = string.Empty;    // "Raw Material Workshop"
        public string? Location { get; set; }
        public decimal CostPerHour { get; set; }
        public int CapacityPerHour { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public virtual ICollection<Equipment> Equipment { get; set; } = new List<Equipment>();
        public virtual ICollection<ProcessRoute> ProcessRoutes { get; set; } = new List<ProcessRoute>();
    }
}
