using Microsoft.AspNetCore.Mvc;
using MyERP.Services.Inventory.DTOs;
using MyERP.Services.Inventory.DTOs.Units;
using MyERP.Services.Inventory.Services.Units;

namespace MyERP.Services.Inventory.Controllers
{
    [ApiController]
    [Route("api/inventory/units")]
    public class UnitsController : ControllerBase
    {
        private readonly IUnitService _unitService;

        public UnitsController(IUnitService unitService)
        {
            _unitService = unitService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUnitDto dto)
        {
            var result = await _unitService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id },
                ApiResponse<UnitResponseDto>.Ok(result, "Unit created successfully"));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _unitService.GetAllAsync();
            return Ok(ApiResponse<List<UnitResponseDto>>.Ok(result));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _unitService.GetByIdAsync(id);
            return Ok(ApiResponse<UnitResponseDto>.Ok(result));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CreateUnitDto dto)
        {
            var result = await _unitService.UpdateAsync(id, dto);
            return Ok(ApiResponse<UnitResponseDto>.Ok(result, "Unit updated successfully"));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _unitService.DeleteAsync(id);
            return Ok(ApiResponse.Ok("Unit deleted successfully"));
        }

        [HttpPatch("{id}/restore")]
        public async Task<IActionResult> Restore(Guid id)
        {
            await _unitService.RestoreAsync(id);
            return Ok(ApiResponse.Ok("Unit restored successfully"));
        }
    }
}
