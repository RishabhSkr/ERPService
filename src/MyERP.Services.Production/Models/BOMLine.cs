/*
 * BOMLine - Individual ingredients/components in a BOM
 * 
 * 📚 INDUSTRY vs NOOB:
 * 
 * ❌ NOOB: Put all components as JSON array in BOM table
 * ✅ INDUSTRY:
 *    1. Separate table for lines (proper normalization)
 *    2. Each line has its own ID (for updates/deletes)
 *    3. LineNumber for ordering
 *    4. Link to RawMaterial (not Product) - these are components, not finished goods
 */

namespace MyERP.Services.Production.Models
{
    /// <summary>
    /// BOM Line Item - One component needed to make the product
    /// Example: "4 wheels" or "1kg fabric"
    /// </summary>
    public class BOMLine
    {
        public Guid BOMLineId { get; set; }
        
        // Parent BOM
        public Guid BOMId { get; set; }
        
        /// <summary>
        /// Line sequence for ordering: 1, 2, 3...
        /// </summary>
        public int LineNumber { get; set; }
        
        /// <summary>
        /// RawMaterialId from Inventory Service - what component is needed
        /// 📝 Industry: Components are Raw Materials, not finished Products
        /// </summary>
        public Guid RawMaterialId { get; set; }
        
        /// <summary>
        /// Denormalized for display - avoids cross-service calls
        /// </summary>
        public string MaterialCode { get; set; } = string.Empty;
        public string MaterialName { get; set; } = string.Empty;
        
        /// <summary>
        /// How much is needed for 1 unit of finished product
        /// Example: 4 wheels per chair
        /// </summary>
        public decimal Quantity { get; set; }
        
        /// <summary>
        /// Unit of measure: pcs, kg, m, L
        /// </summary>
        public string Unit { get; set; } = "pcs";
        
        /// <summary>
        /// Optional: Scrap percentage to account for waste
        /// Example: 5% scrap means order 5% extra
        /// </summary>
        public decimal ScrapPercentage { get; set; } = 0;

        /// <summary>
        /// Which process consumes this material (FK → Process table)
        /// 📝 Industry: BOM defines material-to-process mapping, not ProcessRoute
        /// Example: "Steel Rod" consumed in "1010 - Cutting" process
        /// </summary>
        public Guid? ProcessId { get; set; }

        /// <summary>
        /// Denormalized process code for display: "1010"
        /// </summary>
        public string? ProcessCode { get; set; }

        /// <summary>
        /// Denormalized process name for display: "Cutting"
        /// </summary>
        public string? ProcessName { get; set; }

        // Navigation
        public virtual BOM? BOM { get; set; }
        public virtual Process? Process { get; set; }
    }
}
