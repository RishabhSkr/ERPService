using Microsoft.EntityFrameworkCore;
using MyERP.Services.Inventory.Data;
using MyERP.Services.Inventory.Models;

namespace MyERP.Services.Inventory.Repositories.Categories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly InventoryDbContext _context;

        public CategoryRepository(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<Category?> GetByIdAsync(Guid id)
        {
            return await _context.Categories.FindAsync(id);
        }

        public async Task<Category?> GetByIdWithRelationsAsync(Guid id)
        {
            return await _context.Categories
                .Include(c => c.Products)
                .Include(c => c.RawMaterials)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Category?> GetByCodeAsync(string code)
        {
            return await _context.Categories.FirstOrDefaultAsync(c => c.CategoryCode == code);
        }

        public async Task<List<Category>> GetAllActiveAsync()
        {
            return await _context.Categories
                .Include(c => c.Products)
                .Include(c => c.RawMaterials)
                .Where(c => c.IsActive)
                .OrderBy(c => c.CategoryName)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Categories.AnyAsync(c => c.Id == id);
        }

        public async Task<bool> CodeExistsAsync(string code, Guid? excludeId = null)
        {
            var query = _context.Categories.Where(c => c.CategoryCode == code);
            if (excludeId.HasValue)
                query = query.Where(c => c.Id != excludeId.Value);
            return await query.AnyAsync();
        }

        public async Task<Category> AddAsync(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task UpdateAsync(Category category)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetProductCountAsync(Guid categoryId)
        {
            return await _context.Products.CountAsync(p => p.CategoryId == categoryId);
        }

        public async Task<int> GetRawMaterialCountAsync(Guid categoryId)
        {
            return await _context.RawMaterials.CountAsync(r => r.CategoryId == categoryId);
        }
    }
}
