using Microsoft.AspNetCore.Mvc;
using MyERP.Services.Inventory.DTOs;
using MyERP.Services.Inventory.DTOs.RawMaterials;
using MyERP.Services.Inventory.Services.RawMaterials;
using Microsoft.AspNetCore.Authorization;

namespace MyERP.Services.Inventory.Controllers
{
    [ApiController]
    [Route("api/inventory/raw-materials")]
    [Authorize]
    public class RawMaterialsController : ControllerBase
    {
        private readonly IRawMaterialService _rawMaterialService;

        public RawMaterialsController(IRawMaterialService rawMaterialService)
        {
            _rawMaterialService = rawMaterialService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRawMaterialDto dto)
        {
            var result = await _rawMaterialService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id },
                ApiResponse<RawMaterialResponseDto>.Ok(result, "Raw material created successfully"));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] Guid? categoryId = null,
            [FromQuery] string? searchKeyword = null)
        {
            var result = await _rawMaterialService.GetAllAsync(pageNumber, pageSize, categoryId, searchKeyword);
            return Ok(ApiResponse<PagedResponse<RawMaterialListDto>>.Ok(result));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _rawMaterialService.GetByIdAsync(id);
            return Ok(ApiResponse<RawMaterialResponseDto>.Ok(result));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRawMaterialDto dto)
        {
            var result = await _rawMaterialService.UpdateAsync(id, dto);
            return Ok(ApiResponse<RawMaterialResponseDto>.Ok(result, "Raw material updated successfully"));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _rawMaterialService.DeleteAsync(id);
            return Ok(ApiResponse.Ok("Raw material deleted successfully"));
        }

        [HttpPatch("{id}/restore")]
        public async Task<IActionResult> Restore(Guid id)
        {
            await _rawMaterialService.RestoreAsync(id);
            return Ok(ApiResponse.Ok("Raw material restored successfully"));
        }

        [HttpPost("reserve")]
        public async Task<IActionResult> Reserve([FromBody] ReserveRawMaterialsDto dto)
        {
            var result = await _rawMaterialService.ReserveMaterialsAsync(dto);
            return Ok(ApiResponse<ReservationResponseDto>.Ok(result));
        }

        [HttpPost("release")]
        public async Task<IActionResult> Release([FromBody] ReleaseRawMaterialsDto dto)
        {
            var result = await _rawMaterialService.ReleaseMaterialsAsync(dto);
            return Ok(ApiResponse<ReservationResponseDto>.Ok(result));
        }

        [HttpPost("{id}/add-stock")]
        public async Task<IActionResult> AddStock(Guid id, [FromBody] AddRawMaterialStockDto dto)
        {
            await _rawMaterialService.AddStockAsync(id, dto.StorageLocationId, dto.Quantity, dto.BatchNumber);
            return Ok(ApiResponse.Ok("Stock added successfully"));
        }
    }

    public class AddRawMaterialStockDto
    {
        public Guid StorageLocationId { get; set; }
        public decimal Quantity { get; set; }
        public string? BatchNumber { get; set; }
    }
}
