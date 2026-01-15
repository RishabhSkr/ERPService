using MyERP.Services.Inventory.DTOs;
using MyERP.Services.Inventory.DTOs.Products;
using MyERP.Services.Inventory.Exceptions;
using MyERP.Services.Inventory.Models;
using MyERP.Services.Inventory.Repositories.Categories;
using MyERP.Services.Inventory.Repositories.Products;
using MyERP.Services.Inventory.Repositories.Units;
using MyERP.Services.Inventory.Repositories.Warehouses;

namespace MyERP.Services.Inventory.Services.Products
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly IWarehouseRepository _warehouseRepository;

        public ProductService(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IUnitRepository unitRepository,
            IWarehouseRepository warehouseRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _unitRepository = unitRepository;
            _warehouseRepository = warehouseRepository;
        }

        public async Task<ProductResponseDto> CreateAsync(CreateProductDto dto)
        {
            // Check if code exists
            var exists = await _productRepository.CodeExistsAsync(dto.ProductCode);
            if (exists)
                throw new ConflictException($"Product with code '{dto.ProductCode}' already exists");

            // Validate category exists
            var categoryExists = await _categoryRepository.ExistsAsync(dto.CategoryId);
            if (!categoryExists)
                throw new NotFoundException("Category", dto.CategoryId);

            // Validate unit exists
            var unitExists = await _unitRepository.ExistsAsync(dto.UnitId);
            if (!unitExists)
                throw new NotFoundException("Unit", dto.UnitId);

            var product = new Product
            {
                Id = Guid.NewGuid(),
                ProductCode = dto.ProductCode.ToUpper(),
                ProductName = dto.ProductName,
                Description = dto.Description,
                CategoryId = dto.CategoryId,
                UnitId = dto.UnitId,
                Price = dto.Price,
                MinStockLevel = dto.MinStockLevel,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _productRepository.AddAsync(product);

            return await GetByIdAsync(product.Id);
        }

        public async Task<PagedResponse<ProductListDto>> GetAllAsync(
            int pageNumber = 1,
            int pageSize = 10,
            Guid? categoryId = null,
            bool? isActive = null,
            string? searchKeyword = null)
        {
            var (products, totalCount) = await _productRepository.GetAllAsync(
                pageNumber, pageSize, categoryId, isActive, searchKeyword);

            var data = products.Select(p => new ProductListDto
            {
                Id = p.Id,
                ProductCode = p.ProductCode,
                ProductName = p.ProductName,
                CategoryName = p.Category?.CategoryName ?? "",
                Price = p.Price,
                CurrentStock = p.ProductInventories?.Sum(i => i.CurrentStock) ?? 0,
                ReservedStock = p.ProductInventories?.Sum(i => i.ReservedStock) ?? 0,
                AvailableStock = p.ProductInventories?.Sum(i => i.CurrentStock - i.ReservedStock) ?? 0,
                UnitName = p.Unit?.UnitName ?? "",
                IsActive = p.IsActive
            }).ToList();

            return new PagedResponse<ProductListDto>(data, pageNumber, pageSize, totalCount);
        }

        public async Task<ProductResponseDto> GetByIdAsync(Guid id)
        {
            var product = await _productRepository.GetByIdWithInventoryAsync(id);
            if (product == null)
                throw new NotFoundException("Product", id);

            return MapToDto(product);
        }

        public async Task<ProductResponseDto> UpdateAsync(Guid id, UpdateProductDto dto)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                throw new NotFoundException("Product", id);

            if (!string.IsNullOrWhiteSpace(dto.ProductName))
                product.ProductName = dto.ProductName;

            if (dto.Description != null)
                product.Description = dto.Description;

            if (dto.Price.HasValue)
                product.Price = dto.Price.Value;

            if (dto.MinStockLevel.HasValue)
                product.MinStockLevel = dto.MinStockLevel.Value;

            if (dto.IsActive.HasValue)
                product.IsActive = dto.IsActive.Value;

            product.UpdatedAt = DateTime.UtcNow;
            await _productRepository.UpdateAsync(product);

            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                throw new NotFoundException("Product", id);

            // Soft delete
            product.IsActive = false;
            product.UpdatedAt = DateTime.UtcNow;
            await _productRepository.UpdateAsync(product);

            return true;
        }

        public async Task<bool> RestoreAsync(Guid id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                throw new NotFoundException("Product", id);

            if (product.IsActive)
                throw new BadRequestException("Product is already active");

            // Restore soft-deleted product
            product.IsActive = true;
            product.UpdatedAt = DateTime.UtcNow;
            await _productRepository.UpdateAsync(product);

            return true;
        }

        public async Task<ProductAvailabilityDto> CheckAvailabilityAsync(Guid productId, decimal quantity)
        {
            var product = await _productRepository.GetByIdWithInventoryAsync(productId);
            if (product == null)
                throw new NotFoundException("Product", productId);

            var availableStock = product.ProductInventories?.Sum(i => i.CurrentStock - i.ReservedStock) ?? 0;
            var isAvailable = availableStock >= quantity;

            return new ProductAvailabilityDto
            {
                ProductId = product.Id,
                ProductName = product.ProductName,
                RequestedQuantity = quantity,
                AvailableStock = availableStock,
                IsAvailable = isAvailable,
                Message = isAvailable
                    ? "Stock available"
                    : $"Insufficient stock. Available: {availableStock}, Requested: {quantity}"
            };
        }

        public async Task<bool> AddStockAsync(Guid productId, Guid warehouseId, decimal quantity, string? batchNumber = null)
        {
            var productExists = await _productRepository.ExistsAsync(productId);
            if (!productExists)
                throw new NotFoundException("Product", productId);

            var warehouseExists = await _warehouseRepository.ExistsAsync(warehouseId);
            if (!warehouseExists)
                throw new NotFoundException("Warehouse", warehouseId);

            // Find or create inventory record
            var inventory = await _productRepository.GetInventoryAsync(productId, warehouseId, batchNumber);

            if (inventory == null)
            {
                inventory = new ProductInventory
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId,
                    WarehouseId = warehouseId,
                    CurrentStock = 0,
                    ReservedStock = 0,
                    BatchNumber = batchNumber,
                    CreatedAt = DateTime.UtcNow
                };
                await _productRepository.AddInventoryAsync(inventory);
            }

            inventory.CurrentStock += quantity;
            inventory.UpdatedAt = DateTime.UtcNow;
            await _productRepository.UpdateInventoryAsync(inventory);

            return true;
        }

        private static ProductResponseDto MapToDto(Product product)
        {
            var inventories = product.ProductInventories ?? new List<ProductInventory>();

            return new ProductResponseDto
            {
                Id = product.Id,
                ProductCode = product.ProductCode,
                ProductName = product.ProductName,
                Description = product.Description,
                Category = product.Category != null ? new CategoryInfo
                {
                    Id = product.Category.Id,
                    CategoryName = product.Category.CategoryName,
                    CategoryCode = product.Category.CategoryCode
                } : null,
                Unit = product.Unit != null ? new UnitInfo
                {
                    Id = product.Unit.Id,
                    UnitName = product.Unit.UnitName,
                    UnitCode = product.Unit.UnitCode
                } : null,
                Price = product.Price,
                MinStockLevel = product.MinStockLevel,
                TotalStock = inventories.Sum(i => i.CurrentStock),
                TotalReserved = inventories.Sum(i => i.ReservedStock),
                TotalAvailable = inventories.Sum(i => i.CurrentStock - i.ReservedStock),
                IsActive = product.IsActive,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                WarehouseStock = inventories.Select(i => new WarehouseStockInfo
                {
                    WarehouseId = i.WarehouseId,
                    WarehouseName = i.Warehouse?.WarehouseName ?? "",
                    CurrentStock = i.CurrentStock,
                    ReservedStock = i.ReservedStock,
                    AvailableStock = i.CurrentStock - i.ReservedStock
                }).ToList()
            };
        }
    }
}
