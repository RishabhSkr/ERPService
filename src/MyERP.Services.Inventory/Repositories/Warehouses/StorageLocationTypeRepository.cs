using Microsoft.EntityFrameworkCore;
using MyERP.Services.Inventory.Data;
using MyERP.Services.Inventory.Models;

namespace MyERP.Services.Inventory.Repositories.Warehouses
{
    public class StorageLocationTypeRepository : IStorageLocationTypeRepository
    {
        private readonly InventoryDbContext _context;

        public StorageLocationTypeRepository(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<StorageLocationType>> GetAllAsync()
        {
            return await _context.StorageLocationTypes
                .OrderBy(t => t.TypeName)
                .ToListAsync();
        }

        public async Task<StorageLocationType?> GetByIdAsync(Guid id)
        {
            return await _context.StorageLocationTypes.FindAsync(id);
        }

        public async Task<StorageLocationType> AddAsync(StorageLocationType type)
        {
            _context.StorageLocationTypes.Add(type);
            await _context.SaveChangesAsync();
            return type;
        }

        public async Task UpdateAsync(StorageLocationType type)
        {
            _context.Entry(type).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var type = await _context.StorageLocationTypes.FindAsync(id);
            if (type != null)
            {
                _context.StorageLocationTypes.Remove(type);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.StorageLocationTypes.AnyAsync(e => e.Id == id);
        }

        public async Task<bool> TypeCodeExistsAsync(string code, Guid? excludeId = null)
        {
            var query = _context.StorageLocationTypes.Where(t => t.TypeCode.ToLower() == code.ToLower());
            if (excludeId.HasValue)
            {
                query = query.Where(t => t.Id != excludeId.Value);
            }
            return await query.AnyAsync();
        }
    }
}
