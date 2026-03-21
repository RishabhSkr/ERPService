namespace MyERP.Services.Inventory.DTOs.RawMaterials
{
    public class UpdateRawMaterialDto
    {
        public string? MaterialName { get; set; }
        public string? Description { get; set; }
        public decimal? Cost { get; set; }
        public decimal? MinStockLevel { get; set; }
        public string? Supplier { get; set; }
        public bool? IsActive { get; set; }
    }
}
