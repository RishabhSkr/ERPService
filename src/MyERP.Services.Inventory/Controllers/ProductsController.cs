using Microsoft.AspNetCore.Mvc;
using MyERP.Services.Inventory.DTOs;
using MyERP.Services.Inventory.DTOs.Products;
using MyERP.Services.Inventory.Services.Products;

namespace MyERP.Services.Inventory.Controllers
{
    [ApiController]
    [Route("api/inventory/products")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
        {
            var result = await _productService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id },
                ApiResponse<ProductResponseDto>.Ok(result, "Product created successfully"));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] Guid? categoryId = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] string? searchKeyword = null)
        {
            var result = await _productService.GetAllAsync(pageNumber, pageSize, categoryId, isActive, searchKeyword);
            return Ok(ApiResponse<PagedResponse<ProductListDto>>.Ok(result));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _productService.GetByIdAsync(id);
            return Ok(ApiResponse<ProductResponseDto>.Ok(result));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductDto dto)
        {
            var result = await _productService.UpdateAsync(id, dto);
            return Ok(ApiResponse<ProductResponseDto>.Ok(result, "Product updated successfully"));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _productService.DeleteAsync(id);
            return Ok(ApiResponse.Ok("Product deleted successfully"));
        }

        [HttpPatch("{id}/restore")]
        public async Task<IActionResult> Restore(Guid id)
        {
            await _productService.RestoreAsync(id);
            return Ok(ApiResponse.Ok("Product restored successfully"));
        }

        [HttpGet("{id}/check-availability")]
        public async Task<IActionResult> CheckAvailability(Guid id, [FromQuery] decimal quantity)
        {
            var result = await _productService.CheckAvailabilityAsync(id, quantity);
            return Ok(ApiResponse<ProductAvailabilityDto>.Ok(result));
        }

        [HttpPost("{id}/add-stock")]
        public async Task<IActionResult> AddStock(Guid id, [FromBody] AddStockDto dto)
        {
            await _productService.AddStockAsync(id, dto.WarehouseId, dto.Quantity, dto.BatchNumber);
            return Ok(ApiResponse.Ok("Stock added successfully"));
        }
    }

    public class AddStockDto
    {
        public Guid WarehouseId { get; set; }
        public decimal Quantity { get; set; }
        public string? BatchNumber { get; set; }
    }
}
