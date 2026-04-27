using Microsoft.AspNetCore.Mvc;
using MyERP.Services.Inventory.DTOs.Warehouse;
using MyERP.Services.Inventory.Services.Warehouses;
using Microsoft.AspNetCore.Authorization;

namespace MyERP.Services.Inventory.Controllers
{
    [ApiController]
    [Route("api/inventory/[controller]")]
    [Authorize]
    public class StorageLocationTypesController : ControllerBase
    {
        private readonly IStorageLocationTypeService _service;

        public StorageLocationTypesController(IStorageLocationTypeService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<StorageLocationTypeDto>>> GetAll()
        {
            var types = await _service.GetAllAsync();
            return Ok(types);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<StorageLocationTypeDto>> GetById(Guid id)
        {
            var type = await _service.GetByIdAsync(id);
            return Ok(type);
        }

        [HttpPost]
        public async Task<ActionResult<StorageLocationTypeDto>> Create(CreateStorageLocationTypeDto dto)
        {
            var type = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = type.Id }, type);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<StorageLocationTypeDto>> Update(Guid id, UpdateStorageLocationTypeDto dto)
        {
            var type = await _service.UpdateAsync(id, dto);
            return Ok(type);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
}
