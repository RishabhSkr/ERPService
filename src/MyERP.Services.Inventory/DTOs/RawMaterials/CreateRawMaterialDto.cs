namespace MyERP.Services.Inventory.DTOs.RawMaterials
{
    public class CreateRawMaterialDto
    {
        public string MaterialCode { get; set; } = string.Empty;
        public string MaterialName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid CategoryId { get; set; }
        public Guid UnitId { get; set; }
        public decimal Cost { get; set; }
        public decimal MinStockLevel { get; set; }
        public string? Supplier { get; set; }
        public Guid? DefaultStorageLocationId { get; set; }
    }
}
