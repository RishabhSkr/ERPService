/*
 * BOM DTOs (Data Transfer Objects)
 * 
 * 📚 INDUSTRY vs NOOB:
 * 
 * ❌ NOOB: Use entities directly in controllers (exposes internal structure)
 * ✅ INDUSTRY:
 *    1. Separate DTOs for input (Create) and output (Response)
 *    2. Never expose entity directly to API
 *    3. DTOs can evolve independently from entities
 */

namespace MyERP.Services.Production.DTOs.BOM
{
    // ====================================
    // INPUT DTOs
    // ====================================
    
    /// <summary>
    /// DTO for creating a new BOM
    /// </summary>
    public class CreateBOMDto
    {
        public Guid ProductId { get; set; }
        public string BOMCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<CreateBOMLineDto> Lines { get; set; } = new();
    }

    public class CreateBOMLineDto
    {
        public Guid RawMaterialId { get; set; }
        public string MaterialCode { get; set; } = string.Empty;
        public string MaterialName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = "pcs";
        public decimal ScrapPercentage { get; set; } = 0;
    }

    /// <summary>
    /// DTO for updating an existing BOM
    /// </summary>
    public class UpdateBOMDto
    {
        public string? Description { get; set; }
        public List<UpdateBOMLineDto>? Lines { get; set; }
    }

    public class UpdateBOMLineDto
    {
        public Guid? BOMLineId { get; set; }  // Null for new lines
        public Guid RawMaterialId { get; set; }
        public string MaterialCode { get; set; } = string.Empty;
        public string MaterialName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = "pcs";
        public decimal ScrapPercentage { get; set; } = 0;
    }

    // ====================================
    // OUTPUT DTOs
    // ====================================
    
    /// <summary>
    /// DTO for returning BOM details
    /// </summary>
    public class BOMDto
    {
        public Guid BOMId { get; set; }
        public Guid ProductId { get; set; }
        public string BOMCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int Version { get; set; }
        public bool IsActive { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<BOMLineDto> Lines { get; set; } = new();
    }

    public class BOMLineDto
    {
        public Guid BOMLineId { get; set; }
        public int LineNumber { get; set; }
        public Guid RawMaterialId { get; set; }
        public string MaterialCode { get; set; } = string.Empty;
        public string MaterialName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
        public decimal ScrapPercentage { get; set; }
    }
}
