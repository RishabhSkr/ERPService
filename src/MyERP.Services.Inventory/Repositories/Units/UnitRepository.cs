using Microsoft.EntityFrameworkCore;
using MyERP.Services.Inventory.Data;
using MyERP.Services.Inventory.Models;

namespace MyERP.Services.Inventory.Repositories.Units
{
    public class UnitRepository : IUnitRepository
    {
        private readonly InventoryDbContext _context;

        public UnitRepository(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<Unit?> GetByIdAsync(Guid id)
        {
            return await _context.Units.FindAsync(id);
        }

        public async Task<Unit?> GetByCodeAsync(string code)
        {
            return await _context.Units.FirstOrDefaultAsync(u => u.UnitCode == code);
        }

        public async Task<List<Unit>> GetAllActiveAsync()
        {
            return await _context.Units
                .Where(u => u.IsActive)
                .OrderBy(u => u.UnitName)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Units.AnyAsync(u => u.Id == id);
        }

        public async Task<bool> CodeExistsAsync(string code, Guid? excludeId = null)
        {
            var query = _context.Units.Where(u => u.UnitCode == code);
            if (excludeId.HasValue)
                query = query.Where(u => u.Id != excludeId.Value);
            return await query.AnyAsync();
        }

        public async Task<Unit> AddAsync(Unit unit)
        {
            _context.Units.Add(unit);
            await _context.SaveChangesAsync();
            return unit;
        }

        public async Task UpdateAsync(Unit unit)
        {
            _context.Units.Update(unit);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsInUseAsync(Guid unitId)
        {
            var productExists = await _context.Products.AnyAsync(p => p.UnitId == unitId);
            var rawMaterialExists = await _context.RawMaterials.AnyAsync(r => r.UnitId == unitId);
            return productExists || rawMaterialExists;
        }
    }
}
