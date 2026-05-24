using Microsoft.EntityFrameworkCore;
using MyERP.Services.Inventory.Data;
using MyERP.Services.Inventory.Models;

namespace MyERP.Services.Inventory.Repositories.Warehouses
{
    public class StorageLocationRepository : IStorageLocationRepository
    {
        private readonly InventoryDbContext _context;

        public StorageLocationRepository(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<StorageLocation?> GetByIdAsync(Guid id)
        {
            return await _context.StorageLocations
                .Include(l => l.Warehouse)
                .Include(l => l.LocationType)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<IEnumerable<StorageLocation>> GetByWarehouseIdAsync(Guid warehouseId)
        {
            return await _context.StorageLocations
                .Include(l => l.Warehouse)
                .Include(l => l.LocationType)
                .Where(l => l.WarehouseId == warehouseId)
                .OrderBy(l => l.LocationCode)
                .ToListAsync();
        }

        public async Task<IEnumerable<StorageLocation>> GetAllAsync()
        {
            return await _context.StorageLocations
                .Include(l => l.Warehouse)
                .Include(l => l.LocationType)
                .OrderBy(l => l.WarehouseId)
                .ThenBy(l => l.LocationCode)
                .ToListAsync();
        }

        public async Task<StorageLocation> AddAsync(StorageLocation location)
        {
            await _context.StorageLocations.AddAsync(location);
            await _context.SaveChangesAsync();
            return location;
        }

        public async Task UpdateAsync(StorageLocation location)
        {
            _context.StorageLocations.Update(location);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var location = await _context.StorageLocations.FindAsync(id);
            if (location != null)
            {
                _context.StorageLocations.Remove(location);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.StorageLocations.AnyAsync(l => l.Id == id);
        }

        public async Task<bool> LocationCodeExistsAsync(string locationCode, Guid warehouseId, Guid? excludeId = null)
        {
            var query = _context.StorageLocations.Where(l => l.LocationCode == locationCode && l.WarehouseId == warehouseId);
            if (excludeId.HasValue)
            {
                query = query.Where(l => l.Id != excludeId.Value);
            }
            return await query.AnyAsync();
        }
    }
}
