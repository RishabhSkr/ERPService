using System.Data;
using MyERP.SalesServiceTutorial.Models;
using MyERP.SalesServiceTutorial.Repositories.Customers;

namespace MyERP.SalesServiceTutorial.Services.Customers;

public class CustomerService : ICustomerService
{
    private readonly  ICustomersRepository _repository;

    public CustomerService(ICustomersRepository repository)
    {
        _repository = repository;
    }
    public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
    {

        return await _repository.GetAllAsync();
    }
    
    public async Task<Customer?> GetCustomerByIdAsync(int id)
    {
        var customer = await _repository.GetByIdAsync(id);
        if (customer == null)
            throw new KeyNotFoundException($"Customer with id {id} not found");
        return customer;
    }
    
    public async Task<Customer?> GetCustomerByEmailAsync(string email)
    {
        return await _repository.GetByEmailAsync(email);
    }
    
    public async Task<Customer?> CreateCustomerAsync(Customer customer)
    {
       var existingCustomer = await _repository.GetByEmailAsync(customer.Email);
        if(existingCustomer != null ) throw new InvalidOperationException("Customer with email " + customer.Email + " already exists");
        customer.CreatedAt = DateTime.UtcNow;
        await _repository.AddAsync(customer);
        return customer;
    }
    
    public async Task<Customer> UpdateCustomerAsync(int id, Customer customer)
    {
        var existingCustomer = await _repository.GetByIdAsync(id);
        if(existingCustomer == null ) throw new KeyNotFoundException("Customer not found");
        existingCustomer.Name = customer.Name;
        existingCustomer.Email = customer.Email;
        existingCustomer.Phone = customer.Phone;
        existingCustomer.Address = customer.Address;
        existingCustomer.City = customer.City;
        existingCustomer.Country = customer.Country;
        existingCustomer.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(existingCustomer);
        return existingCustomer;
    }
    
    public async Task DeleteCustomerAsync(int id)
    {
        var existingCustomer = await _repository.GetByIdAsync(id);
        if(existingCustomer == null ) throw new KeyNotFoundException("Customer not found");
        await _repository.DeleteAsync(existingCustomer);
    }

}