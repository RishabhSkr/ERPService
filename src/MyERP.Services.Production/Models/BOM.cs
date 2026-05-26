/*
 * BOM (Bill of Materials) - Manufacturing Recipe
 * 
 * ðŸ“š INDUSTRY vs NOOB:
 * 
 * âŒ NOOB: One big table with all data, no versioning
 * âœ… INDUSTRY:
 *    1. Header + Lines pattern (normalized)
 *    2. Version control (can update recipe without losing history)
 *    3. IsActive flag (soft activate/deactivate)
 *    4. Audit fields (CreatedAt, UpdatedAt, CreatedBy)
 *    5. Clear naming conventions (BOMId not Id, BOMCode not Code)
 */

namespace MyERP.Services.Production.Models
{
    /// <summary>
    /// Bill of Materials Header - The recipe to manufacture a product
    /// Example: "Office Chair" needs 4 wheels, 1 seat, 1 hydraulic cylinder
    /// </summary>
    public class BOM
    {
        public Guid BOMId { get; set; }
        
        /// <summary>
        /// ProductId from Inventory Service - what we're making
        /// ðŸ“ Industry: Store only ID, not full product data (microservices boundary)
        /// </summary>
        public Guid ProductId { get; set; }
        
        /// <summary>
        /// Human-readable code: BOM-CHAIR-001
        /// ðŸ“ Industry: Unique code for business users, separate from GUID
        /// </summary>
        public string BomCode { get; set; } = string.Empty;
        
        /// <summary>
        /// Denormalized product name for display
        /// ðŸ“ Industry: Copy at creation time, avoids cross-service calls for display
        /// </summary>
        public string ProductName { get; set; } = string.Empty;
        
        /// <summary>
        /// Version number: 1, 2, 3...
        /// ðŸ“ Industry: When recipe changes, create new version instead of editing
        /// </summary>
        public int Version { get; set; } = 1;
        
        /// <summary>
        /// Only one version should be active for a product
        /// </summary>
        public bool IsActive { get; set; } = true;
        
        public string? Description { get; set; }
        
        // Audit fields
        public Guid? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation - One BOM has many Lines
        public virtual ICollection<BOMLine> Lines { get; set; } = new List<BOMLine>();
    }
}
