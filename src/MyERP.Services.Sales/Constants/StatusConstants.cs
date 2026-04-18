namespace MyERP.Services.Sales.Constants
{
    /// <summary>
    /// Sales Order lifecycle: Draft → Confirmed → Processing → Shipped → Completed
    ///                                       ↓
    ///                                   Cancelled (from any state except Completed)
    /// </summary>
    public static class SalesOrderStatus
    {
        public const string DRAFT = "Draft";
        public const string CONFIRMED = "Confirmed";
        public const string IN_PRODUCTION = "InProduction";
        public const string SHIPPED = "Shipped";
        public const string COMPLETED = "Completed";
        public const string CANCELLED = "Cancelled";
        public const string DELIVERED = "Delivered";
    }

    /// <summary>
    /// Fulfillment Status: NotFulfilled → PartiallyFulfilled → FullyFulfilled
    /// </summary>
    public static class FulfillmentStatus
    {
        public const string NOT_FULFILLED = "NotFulfilled";
        public const string PARTIALLY_FULFILLED = "PartiallyFulfilled";
        public const string FULLY_FULFILLED = "FullyFulfilled";
    }

    /// <summary>
    /// Payment Status: Unpaid → PartiallyPaid → Paid
    /// </summary>
    public static class PaymentStatus
    {
        public const string UNPAID = "Unpaid";
        public const string PARTIALLY_PAID = "PartiallyPaid";
        public const string PAID = "Paid";
    }
}
