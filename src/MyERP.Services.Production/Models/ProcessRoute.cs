namespace MyERP.Services.Production.Models
{
    /// <summary>
    /// ProcessRoute — sequence of processes for manufacturing a product.
    /// User selects processes from Process master data in desired order.
    /// </summary>
    public class ProcessRoute
    {
        public Guid ProcessRouteId { get; set; }
        public string RouteCode { get; set; } = string.Empty;  // "PR-CHAIR-001"
        public Guid ProductId { get; set; }                     // Which product this route is for

        // Associated WorkCenter
        public Guid WorkCenterId { get; set; }
        public virtual WorkCenter? WorkCenter { get; set; }

        public int Version { get; set; } = 1;
        public bool IsActive { get; set; } = true;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public virtual ICollection<ProcessRouteStep> Steps { get; set; } = new List<ProcessRouteStep>();
    }

    /// <summary>
    /// ProcessRouteStep — individual step in a route, links to Process + Equipment
    /// </summary>
    public class ProcessRouteStep
    {
        public Guid ProcessRouteStepId { get; set; }
        public Guid ProcessRouteId { get; set; }
        public virtual ProcessRoute? ProcessRoute { get; set; }

        public int StepNumber { get; set; }  // 10, 20, 30... (gap for inserting)

        // Which process
        public Guid ProcessId { get; set; }
        public virtual Process? Process { get; set; }

        // Which equipment (optional)
        public Guid? EquipmentId { get; set; }
        public virtual Equipment? Equipment { get; set; }

        // Time estimates
        public int SetupTimeMinutes { get; set; }
        public int RunTimePerUnitMinutes { get; set; }
        public string? Notes { get; set; }

        // Navigation — materials used in this step
        public virtual ICollection<ProcessRouteStepMaterial> Materials { get; set; } = new List<ProcessRouteStepMaterial>();
    }

    /// <summary>
    /// ProcessRouteStepMaterial — which BOM material is used at which process step
    /// </summary>
    public class ProcessRouteStepMaterial
    {
        public Guid Id { get; set; }
        public Guid ProcessRouteStepId { get; set; }
        public virtual ProcessRouteStep? ProcessRouteStep { get; set; }

        // Material reference (from BOM)
        public Guid? BOMLineId { get; set; }
        public virtual BOMLine? BOMLine { get; set; }

        public Guid RawMaterialId { get; set; }
        public string MaterialCode { get; set; } = string.Empty;
        public string MaterialName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
    }
}
