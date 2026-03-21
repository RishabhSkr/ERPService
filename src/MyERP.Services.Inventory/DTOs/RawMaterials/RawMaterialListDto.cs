namespace MyERP.Services.Inventory.DTOs.RawMaterials
{
    public class RawMaterialListDto
    {
        public Guid Id { get; set; }
        public string MaterialCode { get; set; } = string.Empty;
        public string MaterialName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public decimal Cost { get; set; }
        public decimal CurrentStock { get; set; }
        public decimal ReservedStock { get; set; }
        public decimal AvailableStock { get; set; }
        public string UnitName { get; set; } = string.Empty;
        public string? Supplier { get; set; }
        public bool IsActive { get; set; }
    }
}
