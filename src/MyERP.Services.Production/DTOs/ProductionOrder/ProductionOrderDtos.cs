/*
 * ProductionOrder DTOs
 */

namespace MyERP.Services.Production.DTOs.ProductionOrder
{
    /// <summary>
    /// DTO for displaying production orders
    /// </summary>
    public class ProductionOrderDto
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public Guid? SalesOrderId { get; set; }
        public string? SalesOrderNumber { get; set; }
        public Guid ProductId { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public Guid BOMId { get; set; }
        public decimal QuantityPlanned { get; set; }
        public decimal QuantityGood { get; set; }
        public decimal QuantityScrap { get; set; }
        public decimal QuantityProduced { get; set; }
        public decimal PercentComplete { get; set; }
        public DateTime PlannedStartDate { get; set; }
        public DateTime PlannedEndDate { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int Priority { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        // ReservationStatus, ReservationFailReason, ReservationAttempts
        public string? ReservationStatus { get; set; }
        public string? ReservationFailReason { get; set; }
        public int ReservationAttempts { get; set; }
        // BomCode, BomVersion
        public string? BomCode { get; set; }
        public int? BomVersion { get; set; }
        // CancelReason, CancelledAt, MaterialsReturned
        public string? CancelReason { get; set; }
        public DateTime? CancelledAt { get; set; }
        public bool MaterialsReturned { get; set; }
        // ReleasedAt, ReleasedBy
        public DateTime? ReleasedAt { get; set; }
        public Guid? ReleasedBy { get; set; }
        public List<MaterialRequirementDto> MaterialRequirements { get; set; } = new();

    }

    public class MaterialRequirementDto
    {
        public Guid Id { get; set; }
        public Guid RawMaterialId { get; set; }
        public string MaterialCode { get; set; } = string.Empty;
        public string MaterialName { get; set; } = string.Empty;
        public decimal QuantityRequired { get; set; }
        public decimal QuantityReserved { get; set; }
        public decimal QuantityConsumed { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for creating manual production order (not from sales)
    /// </summary>
    public class CreateProductionOrderDto
    {
        public Guid ProductId { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public Guid BOMId { get; set; }
        public decimal QuantityPlanned { get; set; }
        public DateTime PlannedStartDate { get; set; }
        public DateTime PlannedEndDate { get; set; }
        public int Priority { get; set; } = 3;
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO for IoT progress updates
    /// </summary>
    public class UpdateProgressDto
    {
        public decimal QuantityCompleted { get; set; }
    }

    /// <summary>
    /// DTO for completing a production batch
    /// </summary>
    public class CompleteBatchDto
    {
        /// <summary>
        /// Good (non-defective) quantity
        /// </summary>
        public decimal QuantityGood { get; set; }
        
        /// <summary>
        /// Scrapped/defective quantity
        /// </summary>
        public decimal QuantityScrap { get; set; }
        
        /// <summary>
        /// Actual materials consumed (for reconciliation)
        /// </summary>
        public List<MaterialConsumedDto>? MaterialsConsumed { get; set; }
    }

    public class MaterialConsumedDto
    {
        public Guid RawMaterialId { get; set; }
        public decimal QuantityConsumed { get; set; }
    }
}
