using MyERP.SalesServiceTutorial.Models;

namespace MyERP.SalesServiceTutorial.Services.Customers;
public interface ICustomerService
{
    Task<Customer?> GetCustomerByIdAsync(int id);
    Task<IEnumerable<Customer>> GetAllCustomersAsync();
    Task<Customer?> GetCustomerByEmailAsync(string email);
    Task<Customer?>CreateCustomerAsync(Customer customer);
    Task<Customer?>UpdateCustomerAsync(int id,Customer customer);
    Task DeleteCustomerAsync(int id);
}