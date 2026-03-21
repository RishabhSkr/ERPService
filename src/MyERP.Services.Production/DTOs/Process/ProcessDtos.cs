namespace MyERP.Services.Production.DTOs.Process
{
    public class CreateProcessDto
    {
        public string ProcessCode { get; set; } = string.Empty;
        public string ProcessName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;  // Raw, Assembly, Finishing
        public int StandardTimeMinutes { get; set; }
        public string? Description { get; set; }
    }

    public class ProcessDto
    {
        public Guid ProcessId { get; set; }
        public string ProcessCode { get; set; } = string.Empty;
        public string ProcessName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int StandardTimeMinutes { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }
}
