namespace MyERP.Services.Sales.DTOs.SalesOrders
{
    // Dispatch request — which items to dispatch
    public class DispatchRequestDto
    {
        public List<DispatchItemDto> Items { get; set; } = new();
    }

    public class DispatchItemDto
    {
        public Guid ProductId { get; set; }
        public decimal QuantityToDispatch { get; set; }
    }

    // Fulfillment Dashboard response
    public class FulfillmentDashboardDto
    {
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public List<FulfillmentItemDto> Items { get; set; } = new();
    }

    public class FulfillmentItemDto
    {
        public Guid ProductId { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int Ordered { get; set; }
        public decimal Produced { get; set; }
        public decimal AvailableInInventory { get; set; }
        public decimal Dispatched { get; set; }
        public decimal Remaining { get; set; }        // Ordered - Dispatched
        public decimal CanDispatch { get; set; }      // min(Available, Remaining)
        public decimal ProgressPercent { get; set; }  // Produced / Ordered * 100
    }
}
