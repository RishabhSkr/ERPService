/*
 * ProductionOrder Repository Implementation
 */

using Microsoft.EntityFrameworkCore;
using MyERP.Services.Production.Data;
using MyERP.Services.Production.Models;

namespace MyERP.Services.Production.Repositories.ProductionOrders
{
    public class ProductionOrderRepository : IProductionOrderRepository
    {
        private readonly ProductionDbContext _context;

        public ProductionOrderRepository(ProductionDbContext context)
        {
            _context = context;
        }

        public async Task<ProductionOrder?> GetByIdAsync(Guid id)
        {
            return await _context.ProductionOrders.FindAsync(id);
        }

        public async Task<ProductionOrder?> GetByIdWithDetailsAsync(Guid id)
        {
            return await _context.ProductionOrders
                .Include(o => o.MaterialRequirements)
                .Include(o => o.BOM)
                    .ThenInclude(b => b!.Lines)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<IEnumerable<ProductionOrder>> GetAllAsync()
        {
            return await _context.ProductionOrders
                .Include(o => o.MaterialRequirements)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductionOrder>> GetByStatusAsync(string status)
        {
            return await _context.ProductionOrders
                .Include(o => o.MaterialRequirements)
                .Where(o => o.Status == status)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Generate next order number: PO-2026-0001
        /// 📝 Industry Practice: Sequential, year-prefixed, predictable
        /// </summary>
        public async Task<string> GetNextOrderNumberAsync()
        {
            var year = DateTime.UtcNow.Year;
            var prefix = $"PO-{year}-";
            
            var lastOrder = await _context.ProductionOrders
                .Where(o => o.OrderNumber.StartsWith(prefix))
                .OrderByDescending(o => o.OrderNumber)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastOrder != null)
            {
                var lastNumberStr = lastOrder.OrderNumber.Replace(prefix, "");
                if (int.TryParse(lastNumberStr, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"{prefix}{nextNumber:D4}";  // PO-2026-0001
        }

        public async Task<ProductionOrder> CreateAsync(ProductionOrder order)
        {
            _context.ProductionOrders.Add(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<ProductionOrder> UpdateAsync(ProductionOrder order)
        {
            order.UpdatedAt = DateTime.UtcNow;
            _context.ProductionOrders.Update(order);
            await _context.SaveChangesAsync();
            return order;
        }
    }
}
