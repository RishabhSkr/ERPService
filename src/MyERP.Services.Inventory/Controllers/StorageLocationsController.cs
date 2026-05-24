using Microsoft.AspNetCore.Mvc;
using MyERP.Services.Inventory.DTOs.Warehouse;
using MyERP.Services.Inventory.Services.Warehouses;
using Microsoft.AspNetCore.Authorization;

namespace MyERP.Services.Inventory.Controllers
{
    [ApiController]
    [Route("api/inventory/storagelocations")]
    [Authorize]
    public class StorageLocationsController : ControllerBase
    {
        private readonly IStorageLocationService _storageLocationService;
        private readonly ILogger<StorageLocationsController> _logger;

        public StorageLocationsController(IStorageLocationService storageLocationService, ILogger<StorageLocationsController> logger)
        {
            _storageLocationService = storageLocationService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<StorageLocationDto>>> GetAll()
        {
            var locations = await _storageLocationService.GetAllAsync();
            return Ok(locations);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<StorageLocationDto>> GetById(Guid id)
        {
            var location = await _storageLocationService.GetByIdAsync(id);
            return Ok(location);
        }

        [HttpGet("warehouse/{warehouseId}")]
        public async Task<ActionResult<IEnumerable<StorageLocationDto>>> GetByWarehouseId(Guid warehouseId)
        {
            var locations = await _storageLocationService.GetByWarehouseIdAsync(warehouseId);
            return Ok(locations);
        }

        [HttpPost]
        public async Task<ActionResult<StorageLocationDto>> Create(CreateStorageLocationDto dto)
        {
            var location = await _storageLocationService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = location.Id }, location);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<StorageLocationDto>> Update(Guid id, UpdateStorageLocationDto dto)
        {
            var location = await _storageLocationService.UpdateAsync(id, dto);
            return Ok(location);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _storageLocationService.DeleteAsync(id);
            return NoContent();
        }
    }
}
