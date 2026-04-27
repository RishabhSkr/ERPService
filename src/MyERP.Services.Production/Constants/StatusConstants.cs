namespace MyERP.Services.Production.Constants
{
    /// <summary>
    /// Production Order lifecycle: Created → Released → InProgress → Completed
    ///                                       ↓
    ///                                   Cancelled (from any state except Completed)
    /// </summary>
    public static class ProductionOrderStatus
    {
        public const string Create = "Created";
        public const string Released = "Released";
        public const string InProgress = "InProgress";
        public const string Completed = "Completed";
        public const string Cancelled = "Cancelled";
    }

    /// <summary>
    /// Material Reservation: Pending → Reserved / Failed
    /// </summary>
    public static class ReservationStatus
    {
        public const string Pending = "Pending";
        public const string Reserved = "Reserved";
        public const string Failed = "Failed";
    }

    /// <summary>
    /// Pending Request: Pending → Approved → InProduction / Rejected
    /// </summary>
    public static class PendingRequestStatus
    {
        public const string Pending = "Pending";
        public const string Approved = "Approved";
        public const string Rejected = "Rejected";
        public const string Cancelled = "Cancelled";
        public const string InProduction = "InProduction";
    }

    /// <summary>
    /// Work Order: Pending → Released → InProgress → Completed / Cancelled
    /// </summary>
    public static class WorkOrderStatus
    {
        public const string Pending = "Pending";
        public const string Released = "Released";
        public const string InProgress = "InProgress";
        public const string Completed = "Completed";
        public const string Cancelled = "Cancelled";
    }

    /// <summary>
    /// Work Order Execution: Active → Paused → Completed
    /// </summary>
    public static class ExecutionStatus
    {
        public const string Active = "Active";
        public const string Paused = "Paused";
        public const string Completed = "Completed";
    }

    /// <summary>
    /// Equipment: Active / Maintenance / Retired
    /// </summary>
    public static class EquipmentStatus
    {
        public const string Active = "Active";
        public const string Maintenance = "Maintenance";
        public const string Retired = "Retired";
    }

    /// <summary>
    /// Material Requirement: Pending → Reserved → Consumed / Returned
    /// </summary>
    public static class MaterialRequirementStatus
    {
        public const string Pending = "Pending";
        public const string Reserved = "Reserved";
        public const string Consumed = "Consumed";
        public const string Returned = "Returned";
    }
}
