using Microsoft.EntityFrameworkCore;
using MyERP.Services.Inventory.Data;
using MyERP.Services.Inventory.Models;

namespace MyERP.Services.Inventory.Repositories.Inventory
{
    public class StockMovementRepository : IStockMovementRepository
    {
        private readonly InventoryDbContext _context;

        public StockMovementRepository(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<StockMovement> AddAsync(StockMovement movement)
        {
            _context.StockMovements.Add(movement);
            await _context.SaveChangesAsync();
            return movement;
        }

        public async Task<(List<StockMovement> Movements, int TotalCount)> GetAllAsync(
            int pageNumber, int pageSize, string? itemType = null, Guid? itemId = null,
            string? movementType = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.StockMovements.AsQueryable();

            if (!string.IsNullOrWhiteSpace(itemType))
                query = query.Where(m => m.ItemType.ToLower() == itemType.ToLower());

            if (itemId.HasValue)
                query = query.Where(m => m.ItemId == itemId.Value);

            if (!string.IsNullOrWhiteSpace(movementType))
                query = query.Where(m => m.MovementType == movementType.ToUpper());

            if (startDate.HasValue)
                query = query.Where(m => m.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(m => m.CreatedAt <= endDate.Value);

            var totalCount = await query.CountAsync();

            var movements = await query
                .OrderByDescending(m => m.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (movements, totalCount);
        }

        public async Task<StockMovement?> GetByIdAsync(Guid id)
        {
            return await _context.StockMovements.FindAsync(id);
        }

        public async Task<List<StockMovement>> GetByItemAsync(string itemType, Guid itemId, int limit = 50)
        {
            return await _context.StockMovements
                .Where(m => m.ItemType.ToLower() == itemType.ToLower() && m.ItemId == itemId)
                .OrderByDescending(m => m.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }
    }
}
