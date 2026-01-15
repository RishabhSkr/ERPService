namespace MyERP.Services.Inventory.DTOs.RawMaterials
{
    public class ReserveRawMaterialsDto
    {
        public Guid ProductionOrderId { get; set; }
        public List<MaterialReservationItem> Materials { get; set; } = new();
    }

    public class MaterialReservationItem
    {
        public Guid RawMaterialId { get; set; }
        public decimal Quantity { get; set; }
    }

    public class ReservationResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public Guid ProductionOrderId { get; set; }
        public List<ReservedMaterialInfo> Reservations { get; set; } = new();
    }

    public class ReservedMaterialInfo
    {
        public Guid RawMaterialId { get; set; }
        public string MaterialName { get; set; } = string.Empty;
        public decimal ReservedQuantity { get; set; }
        public decimal AvailableStock { get; set; }
    }

    public class ReleaseRawMaterialsDto
    {
        public Guid ProductionOrderId { get; set; }
        public List<MaterialReservationItem> Materials { get; set; } = new();
    }
}
