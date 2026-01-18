using Microsoft.EntityFrameworkCore;
using MyERP.Services.Sales.Data;
using MyERP.Services.Sales.Models;

namespace MyERP.Services.Sales.Repositories.Customers
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly SalesDbContext _context;

        public CustomerRepository(SalesDbContext context)
        {
            _context = context;
        }

        public async Task<Customer?> GetByIdAsync(Guid id)
        {
            return await _context.Customers.FindAsync(id);
        }

        public async Task<Customer?> GetByIdWithOrdersAsync(Guid id)
        {
            return await _context.Customers
                .Include(c => c.SalesOrders)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Customer?> GetByCodeAsync(string code)
        {
            return await _context.Customers.FirstOrDefaultAsync(c => c.CustomerCode == code);
        }

        public async Task<(List<Customer> Customers, int TotalCount)> GetAllAsync(
            int pageNumber, int pageSize, string? searchKeyword = null)
        {
            var query = _context.Customers
                .Where(c => c.IsActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchKeyword))
            {
                var keyword = searchKeyword.ToLower();
                query = query.Where(c =>
                    c.CustomerCode.ToLower().Contains(keyword) ||
                    c.CustomerName.ToLower().Contains(keyword) ||
                    (c.City != null && c.City.ToLower().Contains(keyword)));
            }

            var totalCount = await query.CountAsync();

            var customers = await query
                .OrderBy(c => c.CustomerName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (customers, totalCount);
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Customers.AnyAsync(c => c.Id == id && c.IsActive);
        }

        public async Task<bool> CodeExistsAsync(string code, Guid? excludeId = null)
        {
            var query = _context.Customers.Where(c => c.CustomerCode == code);
            if (excludeId.HasValue)
                query = query.Where(c => c.Id != excludeId.Value);
            return await query.AnyAsync();
        }

        public async Task<Customer> AddAsync(Customer customer)
        {
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            return customer;
        }

        public async Task UpdateAsync(Customer customer)
        {
            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetOrderCountAsync(Guid customerId)
        {
            return await _context.SalesOrders.CountAsync(o => o.CustomerId == customerId);
        }
    }
}
