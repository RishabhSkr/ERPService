/*
 * PendingRequest DTOs
 */

namespace MyERP.Services.Production.DTOs.PendingRequest
{
    /// <summary>
    /// DTO for displaying pending requests
    /// </summary>
    public class PendingRequestDto
    {
        public Guid Id { get; set; }
        public Guid SalesOrderId { get; set; }
        public string SalesOrderNumber { get; set; } = string.Empty;
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime ReceivedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public List<PendingRequestItemDto> Items { get; set; } = new();
    }

    public class PendingRequestItemDto
    {
        public Guid ProductId { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    /// <summary>
    /// DTO for approving a pending request
    /// </summary>
    public class ApproveRequestDto
    {
        /// <summary>
        /// Planned start date for production
        /// </summary>
        public DateTime PlannedStartDate { get; set; }
        
        /// <summary>
        /// Priority: 1 (highest) to 5 (lowest)
        /// </summary>
        public int Priority { get; set; } = 3;
        
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO for cancelling a pending request
    /// </summary>
    public class CancelRequestDto
    {
        public string Reason { get; set; } = string.Empty;
    }
}
