using Microsoft.EntityFrameworkCore;
using MyERP.Services.Sales.Data;
using MyERP.Services.Sales.Models;

namespace MyERP.Services.Sales.Repositories.SalesOrders
{
    public class SalesOrderRepository : ISalesOrderRepository
    {
        private readonly SalesDbContext _context;

        public SalesOrderRepository(SalesDbContext context)
        {
            _context = context;
        }

        public async Task<SalesOrder?> GetByIdAsync(Guid id)
        {
            return await _context.SalesOrders
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<SalesOrder?> GetByIdWithItemsAsync(Guid id)
        {
            return await _context.SalesOrders
                .Include(o => o.Customer)
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<SalesOrder?> GetByOrderNumberAsync(string orderNumber)
        {
            return await _context.SalesOrders
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
        }

        public async Task<(List<SalesOrder> Orders, int TotalCount)> GetAllAsync(
            int pageNumber, int pageSize, string? status = null, Guid? customerId = null)
        {
            var query = _context.SalesOrders
                .Include(o => o.Customer)
                .Include(o => o.Items)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(o => o.OrderStatus == status);

            if (customerId.HasValue)
                query = query.Where(o => o.CustomerId == customerId.Value);

            var totalCount = await query.CountAsync();

            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (orders, totalCount);
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.SalesOrders.AnyAsync(o => o.Id == id);
        }

        public async Task<SalesOrder> AddAsync(SalesOrder order)
        {
            _context.SalesOrders.Add(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task UpdateAsync(SalesOrder order)
        {
            _context.SalesOrders.Update(order);
            await _context.SaveChangesAsync();
        }

        public async Task<string> GenerateOrderNumberAsync()
        {
            var year = DateTime.UtcNow.Year;
            var prefix = $"SO-{year}-";

            var lastOrder = await _context.SalesOrders
                .Where(o => o.OrderNumber.StartsWith(prefix))
                .OrderByDescending(o => o.OrderNumber)
                .FirstOrDefaultAsync();

            if (lastOrder == null)
                return $"{prefix}0001";

            var lastNumber = int.Parse(lastOrder.OrderNumber.Substring(prefix.Length));
            return $"{prefix}{(lastNumber + 1).ToString("D4")}";
        }
    }
}
