using Microsoft.EntityFrameworkCore;
using MyERP.Services.Inventory.Data;
using MyERP.Services.Inventory.Models;

namespace MyERP.Services.Inventory.Repositories.Products
{
    public class ProductRepository : IProductRepository
    {
        private readonly InventoryDbContext _context;

        public ProductRepository(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<Product?> GetByIdAsync(Guid id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Unit)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Product?> GetByIdWithInventoryAsync(Guid id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Unit)
                .Include(p => p.ProductInventories!)
                    .ThenInclude(pi => pi.Warehouse)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Product?> GetByCodeAsync(string code)
        {
            return await _context.Products.FirstOrDefaultAsync(p => p.ProductCode == code);
        }

        public async Task<(List<Product> Products, int TotalCount)> GetAllAsync(
            int pageNumber, int pageSize, Guid? categoryId = null, bool? isActive = null, string? searchKeyword = null)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Unit)
                .Include(p => p.ProductInventories)
                .Where(p => p.IsActive)
                .AsQueryable();

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            if (isActive.HasValue)
                query = query.Where(p => p.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(searchKeyword))
            {
                var keyword = searchKeyword.ToLower();
                query = query.Where(p =>
                    p.ProductCode.ToLower().Contains(keyword) ||
                    p.ProductName.ToLower().Contains(keyword));
            }

            var totalCount = await query.CountAsync();

            var products = await query
                .OrderBy(p => p.ProductName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (products, totalCount);
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Products.AnyAsync(p => p.Id == id);
        }

        public async Task<bool> CodeExistsAsync(string code)
        {
            return await _context.Products.AnyAsync(p => p.ProductCode == code);
        }

        public async Task<Product> AddAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        public async Task<ProductInventory?> GetInventoryAsync(Guid productId, Guid warehouseId, string? batchNumber = null)
        {
            return await _context.ProductInventories
                .FirstOrDefaultAsync(i => i.ProductId == productId && i.WarehouseId == warehouseId && i.BatchNumber == batchNumber);
        }

        public async Task<ProductInventory> AddInventoryAsync(ProductInventory inventory)
        {
            _context.ProductInventories.Add(inventory);
            await _context.SaveChangesAsync();
            return inventory;
        }

        public async Task UpdateInventoryAsync(ProductInventory inventory)
        {
            _context.ProductInventories.Update(inventory);
            await _context.SaveChangesAsync();
        }
    }
}
