namespace MyERP.Services.Inventory.DTOs.Units
{
    public class CreateUnitDto
    {
        public string UnitName { get; set; } = string.Empty;
        public string UnitCode { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
