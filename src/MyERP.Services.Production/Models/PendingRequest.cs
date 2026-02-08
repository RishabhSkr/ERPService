/*
 * PendingRequest - Inbox for Sales Order events
 * 
 * 📚 INDUSTRY vs NOOB:
 * 
 * ❌ NOOB: Process sales order immediately in consumer (risky!)
 * ✅ INDUSTRY: Inbox Pattern
 *    1. Save event to database first (guaranteed persistence)
 *    2. Production manager reviews and approves
 *    3. Then create ProductionOrder
 *    
 *    Benefits:
 *    - Database transaction = no lost messages
 *    - Manual approval = business control
 *    - Audit trail = who approved what, when
 */

namespace MyERP.Services.Production.Models
{
    /// <summary>
    /// Pending Request - Sales orders waiting for production approval
    /// This is the "Inbox" pattern for event-driven architecture
    /// </summary>
    public class PendingRequest
    {
        public Guid Id { get; set; }
        
        /// <summary>
        /// Original Sales Order ID from Sales Service
        /// </summary>
        public Guid SalesOrderId { get; set; }
        
        /// <summary>
        /// Human-readable order number: SO-2026-0001
        /// </summary>
        public string SalesOrderNumber { get; set; } = string.Empty;
        
        /// <summary>
        /// Status: Pending, Approved, Cancelled
        /// 📝 Industry: Use constants or enum, not magic strings
        /// </summary>
        public string Status { get; set; } = PendingRequestStatus.Pending;
        
        // Denormalized customer info for display
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        
        public DateTime OrderDate { get; set; }
        
        /// <summary>
        /// When this request was received from Sales Service
        /// </summary>
        public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// When it was processed (approved/cancelled)
        /// </summary>
        public DateTime? ProcessedAt { get; set; }
        
        /// <summary>
        /// Who processed it
        /// </summary>
        public Guid? ProcessedBy { get; set; }
        
        /// <summary>
        /// Reason for cancellation (if cancelled)
        /// </summary>
        public string? CancellationReason { get; set; }
        
        /// <summary>
        /// Event ID for idempotency check
        /// 📝 Industry: Prevent duplicate processing if same event arrives twice
        /// </summary>
        public Guid EventId { get; set; }
        
        // Navigation
        public virtual ICollection<PendingRequestItem> Items { get; set; } = new List<PendingRequestItem>();
    }
    
    /// <summary>
    /// Individual items in the pending request
    /// </summary>
    public class PendingRequestItem
    {
        public Guid Id { get; set; }
        public Guid PendingRequestId { get; set; }
        
        public Guid ProductId { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        
        // Navigation
        public virtual PendingRequest? PendingRequest { get; set; }
    }
    
    /// <summary>
    /// Status constants - Industry practice over magic strings
    /// </summary>
    public static class PendingRequestStatus
    {
        public const string Pending = "Pending";
        public const string Approved = "Approved";
        public const string Cancelled = "Cancelled";
    }
}
