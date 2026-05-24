using MyERP.Services.Sales.Models;

namespace MyERP.Services.Sales.Repositories.SalesOrders
{
    public interface ISalesOrderRepository
    {
        Task<SalesOrder?> GetByIdAsync(Guid id);
        Task<SalesOrder?> GetByIdWithItemsAsync(Guid id);
        Task<SalesOrder?> GetByOrderNumberAsync(string orderNumber);
        Task<(List<SalesOrder> Orders, int TotalCount)> GetAllAsync(
            int pageNumber, int pageSize, string? status = null, Guid? customerId = null);
        Task<bool> ExistsAsync(Guid id);
        Task<SalesOrder> AddAsync(SalesOrder order);
        Task UpdateAsync(SalesOrder order);
        Task<string> GenerateOrderNumberAsync();
        Task<IEnumerable<SalesOrder>> GetActiveOrdersWithItemsAsync();

    }
}
