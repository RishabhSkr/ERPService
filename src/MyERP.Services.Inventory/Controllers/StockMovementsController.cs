using Microsoft.AspNetCore.Mvc;
using MyERP.Services.Inventory.DTOs;
using MyERP.Services.Inventory.DTOs.StockMovements;
using MyERP.Services.Inventory.Services.StockMovements;
using Microsoft.AspNetCore.Authorization;

namespace MyERP.Services.Inventory.Controllers
{
    [ApiController]
    [Route("api/inventory/stock-movements")]
    [Authorize]
    public class StockMovementsController : ControllerBase
    {
        private readonly IStockMovementService _stockMovementService;

        public StockMovementsController(IStockMovementService stockMovementService)
        {
            _stockMovementService = stockMovementService;
        }

        /// <summary>
        /// Record a new stock movement (IN, OUT, RESERVE, RELEASE, ADJUST, SCRAP)
        /// </summary>
        [HttpPost("record")]
        public async Task<IActionResult> RecordMovement([FromBody] RecordStockMovementDto dto)
        {
            // In real scenario, get userId from JWT token claims
            var result = await _stockMovementService.RecordMovementAsync(dto, null);
            return CreatedAtAction(nameof(GetById), new { id = result.Id },
                ApiResponse<StockMovementResponseDto>.Ok(result, "Stock movement recorded successfully"));
        }

        /// <summary>
        /// Transfer stock between storage locations
        /// </summary>
        [HttpPost("transfer")]
        public async Task<IActionResult> TransferStock([FromBody] TransferStockDto dto)
        {
            var result = await _stockMovementService.TransferStockAsync(dto, null);
            return Ok(ApiResponse<StockMovementResponseDto>.Ok(result, "Stock transferred successfully"));
        }

        /// <summary>
        /// Get all stock movements with filters
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? itemType = null,
            [FromQuery] Guid? itemId = null,
            [FromQuery] string? movementType = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var result = await _stockMovementService.GetAllAsync(
                pageNumber, pageSize, itemType, itemId, movementType, startDate, endDate);
            return Ok(ApiResponse<PagedResponse<StockMovementResponseDto>>.Ok(result));
        }

        /// <summary>
        /// Get stock movement by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _stockMovementService.GetByIdAsync(id);
            return Ok(ApiResponse<StockMovementResponseDto>.Ok(result));
        }

        /// <summary>
        /// Get movement history for a specific item
        /// </summary>
        [HttpGet("item/{itemType}/{itemId}")]
        public async Task<IActionResult> GetByItem(string itemType, Guid itemId)
        {
            var result = await _stockMovementService.GetByItemAsync(itemType, itemId);
            return Ok(ApiResponse<List<StockMovementResponseDto>>.Ok(result));
        }
    }
}
