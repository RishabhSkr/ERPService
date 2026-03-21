namespace MyERP.Services.Production.Models
{
    /// <summary>
    /// Process Master — catalog of operations (1xxx=Raw, 2xxx=Assembly, 3xxx=Finishing)
    /// </summary>
    public class Process
    {
        public Guid ProcessId { get; set; }
        public string ProcessCode { get; set; } = string.Empty;   // "1010", "2030", "3040"
        public string ProcessName { get; set; } = string.Empty;   // "Cutting", "Assembly"
        public string Category { get; set; } = string.Empty;      // "Raw", "Assembly", "Finishing"
        public int StandardTimeMinutes { get; set; }               // Default time per unit
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
