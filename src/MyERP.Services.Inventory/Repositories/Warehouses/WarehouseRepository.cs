using Microsoft.EntityFrameworkCore;
using MyERP.Services.Inventory.Data;
using MyERP.Services.Inventory.Models;

namespace MyERP.Services.Inventory.Repositories.Warehouses
{
    public class WarehouseRepository : IWarehouseRepository
    {
        private readonly InventoryDbContext _context;

        public WarehouseRepository(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<Warehouse?> GetByIdAsync(Guid id)
        {
            return await _context.Warehouses.FindAsync(id);
        }

        public async Task<Warehouse?> GetByIdWithDetailsAsync(Guid id)
        {
            return await _context.Warehouses
                .Include(w => w.StorageLocations!)
                    .ThenInclude(sl => sl.LocationType)
                .Include(w => w.StorageLocations!)
                    .ThenInclude(sl => sl.ProductInventories!)
                        .ThenInclude(pi => pi.Product)
                .Include(w => w.StorageLocations!)
                    .ThenInclude(sl => sl.RawMaterialInventories!)
                        .ThenInclude(ri => ri.RawMaterial)
                .AsSplitQuery()
                .FirstOrDefaultAsync(w => w.Id == id);
        }

        public async Task<Warehouse?> GetFirstActiveAsync()
        {
            return await _context.Warehouses.FirstOrDefaultAsync(w => w.IsActive);
        }

        public async Task<List<Warehouse>> GetAllActiveAsync()
        {
            return await _context.Warehouses
                .Where(w => w.IsActive)
                .OrderBy(w => w.WarehouseName)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Warehouses.AnyAsync(w => w.Id == id);
        }

        public async Task<List<Warehouse>> GetAllAsync()
        {
            return await _context.Warehouses.OrderBy(w => w.WarehouseName).ToListAsync();
        }

        public async Task<Warehouse> AddAsync(Warehouse warehouse)
        {
            await _context.Warehouses.AddAsync(warehouse);
            await _context.SaveChangesAsync();
            return warehouse;
        }

        public async Task<Warehouse> UpdateAsync(Warehouse warehouse)
        {
            _context.Warehouses.Update(warehouse);
            await _context.SaveChangesAsync();
            return warehouse;
        }

        public async Task DeleteAsync(Guid id)
        {
            var warehouse = await GetByIdAsync(id);
            if (warehouse != null)
            {
                _context.Warehouses.Remove(warehouse);
                await _context.SaveChangesAsync();
            }
        }
    }
}
