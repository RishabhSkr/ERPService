/*
 * BOMsController - Bill of Materials API
 * 
 * 📚 INDUSTRY vs NOOB:
 * 
 * ❌ NOOB: Fat controllers with business logic
 * ✅ INDUSTRY:
 *    1. Thin controllers - only HTTP concerns
 *    2. Use service layer for business logic
 *    3. Return consistent ApiResponse format
 *    4. Use proper HTTP verbs and status codes
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyERP.Services.Production.DTOs.BOM;
using MyERP.Services.Production.Middleware;
using MyERP.Services.Production.Services.BOM;
using System.Security.Claims;

namespace MyERP.Services.Production.Controllers
{
    [ApiController]
    [Route("api/production/[controller]")]
    // [Authorize] // Uncomment when JWT is set up
    public class BOMsController : ControllerBase
    {
        private readonly IBOMService _service;
        private readonly ILogger<BOMsController> _logger;

        public BOMsController(IBOMService service, ILogger<BOMsController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Get all BOMs
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<BOMDto>>>> GetAll()
        {
            var boms = await _service.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<BOMDto>>.Ok(boms));
        }

        /// <summary>
        /// Get BOM by ID with lines
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ApiResponse<BOMDto>>> GetById(Guid id)
        {
            var bom = await _service.GetByIdAsync(id);
            return Ok(ApiResponse<BOMDto>.Ok(bom));
        }

        /// <summary>
        /// Get active BOM for a product
        /// </summary>
        [HttpGet("product/{productId:guid}")]
        public async Task<ActionResult<ApiResponse<BOMDto>>> GetByProductId(Guid productId)
        {
            var bom = await _service.GetActiveByProductIdAsync(productId);
            if (bom == null)
                return NotFound(ApiResponse<BOMDto>.Fail($"No active BOM found for product {productId}"));
            
            return Ok(ApiResponse<BOMDto>.Ok(bom));
        }

        /// <summary>
        /// Create a new BOM
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<BOMDto>>> Create([FromBody] CreateBOMDto dto)
        {
            var userId = GetCurrentUserId();
            var bom = await _service.CreateAsync(dto, userId);
            
            return CreatedAtAction(
                nameof(GetById),
                new { id = bom.BOMId },
                ApiResponse<BOMDto>.Ok(bom, "BOM created successfully"));
        }

        /// <summary>
        /// Update an existing BOM
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ApiResponse<BOMDto>>> Update(Guid id, [FromBody] UpdateBOMDto dto)
        {
            var bom = await _service.UpdateAsync(id, dto);
            return Ok(ApiResponse<BOMDto>.Ok(bom, "BOM updated successfully"));
        }

        /// <summary>
        /// Deactivate a BOM (soft delete)
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<ApiResponse<string>>> Deactivate(Guid id)
        {
            await _service.DeactivateAsync(id);
            return Ok(new ApiResponse { Success = true, Message = "BOM deactivated successfully" });
        }

        private Guid? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
}
