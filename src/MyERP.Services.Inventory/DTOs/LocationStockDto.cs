namespace MyERP.Services.Inventory.DTOs
{
    public class LocationStockDto
    {
        public Guid StorageLocationId { get; set; }
        public string LocationCode { get; set; } = string.Empty;
        public string WarehouseName { get; set; } = string.Empty;
        public decimal CurrentStock { get; set; }
        public decimal ReservedStock { get; set; }
        public decimal AvailableStock { get; set; }
    }
}
