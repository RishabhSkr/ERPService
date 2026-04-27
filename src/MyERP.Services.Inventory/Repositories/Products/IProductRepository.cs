using MyERP.Services.Inventory.Models;

namespace MyERP.Services.Inventory.Repositories.Products
{
    public interface IProductRepository
    {
        Task<Product?> GetByIdAsync(Guid id);
        Task<Product?> GetByIdWithInventoryAsync(Guid id);
        Task<Product?> GetByCodeAsync(string code);
        Task<(List<Product> Products, int TotalCount)> GetAllAsync(
            int pageNumber, int pageSize, Guid? categoryId = null, bool? isActive = null, string? searchKeyword = null);
        Task<bool> ExistsAsync(Guid id);
        Task<bool> CodeExistsAsync(string code);
        Task<Product> AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task<ProductInventory?> GetInventoryAsync(Guid productId, Guid storageLocationId, string? batchNumber = null);
        Task AddProductInventoryAsync(ProductInventory inventory);
        Task UpdateInventoryAsync(ProductInventory inventory);
    }
}
