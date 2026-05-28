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
            {
                var start = DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc);
                query = query.Where(m => m.CreatedAt >= start);
            }

            if (endDate.HasValue)
            {
                // End of day = next day 00:00:00 UTC (exclusive) — handles full-day range
                var end = DateTime.SpecifyKind(endDate.Value.Date.AddDays(1), DateTimeKind.Utc);
                query = query.Where(m => m.CreatedAt < end);
            }

            var totalCount = await query.CountAsync();

            var movements = await query
                .Include(m => m.FromLocation)
                .Include(m => m.ToLocation)
                .AsSplitQuery()
                .OrderByDescending(m => m.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (movements, totalCount);
        }

        public async Task<StockMovement?> GetByIdAsync(Guid id)
        {
            return await _context.StockMovements
                .Include(m => m.FromLocation)
                .Include(m => m.ToLocation)
                .AsSplitQuery()
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<List<StockMovement>> GetByItemAsync(string itemType, Guid itemId, int limit = 50)
        {
            return await _context.StockMovements
                .Include(m => m.FromLocation)
                .Include(m => m.ToLocation)
                .AsSplitQuery()
                .Where(m => m.ItemType.ToLower() == itemType.ToLower() && m.ItemId == itemId)
                .OrderByDescending(m => m.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }
    }
}
