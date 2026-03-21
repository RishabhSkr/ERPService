namespace MyERP.Services.Inventory.DTOs.Units
{
    public class UnitResponseDto
    {
        public Guid Id { get; set; }
        public string UnitName { get; set; } = string.Empty;
        public string UnitCode { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
