using MyERP.SalesServiceTutorial.Models;
namespace MyERP.SalesServiceTutorial.Repositories.Customers;

public interface ICustomersRepository
{
    Task<Customer?> GetByIdAsync(int id);
    Task<IEnumerable<Customer>> GetAllAsync();
    Task<Customer?> GetByEmailAsync(string email);
    Task AddAsync(Customer customer);
    Task UpdateAsync(Customer customer);
    Task DeleteAsync(Customer customer);
}