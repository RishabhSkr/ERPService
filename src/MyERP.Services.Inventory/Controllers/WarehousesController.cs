using Microsoft.AspNetCore.Mvc;
using MyERP.Services.Inventory.DTOs;
using MyERP.Services.Inventory.Services.Warehouses;
using Microsoft.AspNetCore.Authorization;

namespace MyERP.Services.Inventory.Controllers
{
    [ApiController]
    [Route("api/inventory/[controller]")]
    [Authorize]
    public class WarehousesController : ControllerBase
    {
        private readonly IWarehouseService _warehouseService;

        public WarehousesController(IWarehouseService warehouseService)
        {
            _warehouseService = warehouseService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<WarehouseDto>>> GetAll()
        {
            var warehouses = await _warehouseService.GetAllAsync();
            return Ok(warehouses);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<WarehouseDto>> GetById(Guid id)
        {
            var warehouse = await _warehouseService.GetByIdAsync(id);
            return Ok(warehouse);
        }

        [HttpPost]
        public async Task<ActionResult<WarehouseDto>> Create(CreateWarehouseDto dto)
        {
            var warehouse = await _warehouseService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = warehouse.Id }, warehouse);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<WarehouseDto>> Update(Guid id, UpdateWarehouseDto dto)
        {
            var warehouse = await _warehouseService.UpdateAsync(id, dto);
            return Ok(warehouse);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _warehouseService.DeleteAsync(id);
            return NoContent();
        }
    }
}
