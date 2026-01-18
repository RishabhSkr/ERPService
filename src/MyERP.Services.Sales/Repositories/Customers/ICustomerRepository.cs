using MyERP.Services.Sales.Models;

namespace MyERP.Services.Sales.Repositories.Customers
{
    public interface ICustomerRepository
    {
        Task<Customer?> GetByIdAsync(Guid id);
        Task<Customer?> GetByIdWithOrdersAsync(Guid id);
        Task<Customer?> GetByCodeAsync(string code);
        Task<(List<Customer> Customers, int TotalCount)> GetAllAsync(int pageNumber, int pageSize, string? searchKeyword = null);
        Task<bool> ExistsAsync(Guid id);
        Task<bool> CodeExistsAsync(string code, Guid? excludeId = null);
        Task<Customer> AddAsync(Customer customer);
        Task UpdateAsync(Customer customer);
        Task<int> GetOrderCountAsync(Guid customerId);
    }
}
