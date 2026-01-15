using Microsoft.EntityFrameworkCore;
using MyERP.Services.Inventory.Data;
using MyERP.Services.Inventory.Models;

namespace MyERP.Services.Inventory.Repositories.RawMaterials
{
    public class RawMaterialRepository : IRawMaterialRepository
    {
        private readonly InventoryDbContext _context;

        public RawMaterialRepository(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<RawMaterial?> GetByIdAsync(Guid id)
        {
            return await _context.RawMaterials
                .Include(r => r.Category)
                .Include(r => r.Unit)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<RawMaterial?> GetByIdWithInventoryAsync(Guid id)
        {
            return await _context.RawMaterials
                .Include(r => r.Category)
                .Include(r => r.Unit)
                .Include(r => r.RawMaterialInventories!)
                    .ThenInclude(ri => ri.Warehouse)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<RawMaterial?> GetByCodeAsync(string code)
        {
            return await _context.RawMaterials.FirstOrDefaultAsync(r => r.MaterialCode == code);
        }

        public async Task<(List<RawMaterial> Materials, int TotalCount)> GetAllAsync(
            int pageNumber, int pageSize, Guid? categoryId = null, string? searchKeyword = null)
        {
            var query = _context.RawMaterials
                .Include(r => r.Category)
                .Include(r => r.Unit)
                .Include(r => r.RawMaterialInventories)
                .Where(r => r.IsActive)
                .AsQueryable();

            if (categoryId.HasValue)
                query = query.Where(r => r.CategoryId == categoryId.Value);

            if (!string.IsNullOrWhiteSpace(searchKeyword))
            {
                var keyword = searchKeyword.ToLower();
                query = query.Where(r =>
                    r.MaterialCode.ToLower().Contains(keyword) ||
                    r.MaterialName.ToLower().Contains(keyword));
            }

            var totalCount = await query.CountAsync();

            var materials = await query
                .OrderBy(r => r.MaterialName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (materials, totalCount);
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.RawMaterials.AnyAsync(r => r.Id == id);
        }

        public async Task<bool> CodeExistsAsync(string code)
        {
            return await _context.RawMaterials.AnyAsync(r => r.MaterialCode == code);
        }

        public async Task<RawMaterial> AddAsync(RawMaterial material)
        {
            _context.RawMaterials.Add(material);
            await _context.SaveChangesAsync();
            return material;
        }

        public async Task UpdateAsync(RawMaterial material)
        {
            _context.RawMaterials.Update(material);
            await _context.SaveChangesAsync();
        }

        public async Task<RawMaterialInventory?> GetInventoryAsync(Guid rawMaterialId, Guid warehouseId, string? batchNumber = null)
        {
            return await _context.RawMaterialInventories
                .Include(i => i.RawMaterial)
                .FirstOrDefaultAsync(i => i.RawMaterialId == rawMaterialId && i.WarehouseId == warehouseId && i.BatchNumber == batchNumber);
        }

        public async Task<List<RawMaterialInventory>> GetInventoriesAsync(Guid rawMaterialId)
        {
            return await _context.RawMaterialInventories
                .Include(i => i.Warehouse)
                .Where(i => i.RawMaterialId == rawMaterialId)
                .ToListAsync();
        }

        public async Task<RawMaterialInventory> AddInventoryAsync(RawMaterialInventory inventory)
        {
            _context.RawMaterialInventories.Add(inventory);
            await _context.SaveChangesAsync();
            return inventory;
        }

        public async Task UpdateInventoryAsync(RawMaterialInventory inventory)
        {
            _context.RawMaterialInventories.Update(inventory);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
