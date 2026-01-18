using Microsoft.EntityFrameworkCore;
using MyERP.SalesServiceTutorial.Data;
using MyERP.SalesServiceTutorial.Models;
namespace MyERP.SalesServiceTutorial.Repositories.Customers;
public class CustomerRepository : ICustomersRepository
{
    private readonly SalesDbContext _context;
    
    // Constructor - DI will inject DbContext
    public CustomerRepository(SalesDbContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<Customer>> GetAllAsync()
    {
        // TODO: Tum likho - Hint: _context.Customers.ToListAsync()
        return await _context.Customers.ToListAsync();
    }
    
    public async Task<Customer?> GetByIdAsync(int id)
    {
        return await _context.Customers.FindAsync(id);
    }
    
    public async Task<Customer?> GetByEmailAsync(string email)
    {
        return await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);
    }
    
    public async Task AddAsync(Customer customer)
    {
        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync();
    }
    
    public async Task UpdateAsync(Customer customer)
    {
        _context.Customers.Update(customer);
        await _context.SaveChangesAsync();
    }
    
    public async Task DeleteAsync(Customer customer)
    {
        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();
    }
}