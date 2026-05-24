using MyERP.Services.Inventory.DTOs;
using MyERP.Services.Inventory.DTOs.Products;

namespace MyERP.Services.Inventory.Services.Products
{
    public interface IProductService
    {
        Task<ProductResponseDto> CreateAsync(CreateProductDto dto);
        Task<PagedResponse<ProductListDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10, Guid? categoryId = null, bool? isActive = null, string? searchKeyword = null);
        Task<ProductResponseDto> GetByIdAsync(Guid id);
        Task<ProductResponseDto> UpdateAsync(Guid id, UpdateProductDto dto);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> RestoreAsync(Guid id);  // Restore soft-deleted product
        Task<ProductAvailabilityDto> CheckAvailabilityAsync(Guid productId, decimal quantity);
        Task<bool> UpdateProductInventoryAsync(Guid productId, Guid storageLocationId, decimal quantity, string? batchNumber = null);
    }
}
