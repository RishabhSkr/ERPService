/*
 * ProductionOrder Repository
 */

using MyERP.Services.Production.Models;

namespace MyERP.Services.Production.Repositories.ProductionOrders
{
    public interface IProductionOrderRepository
    {
        Task<ProductionOrder?> GetByIdAsync(Guid id);
        Task<ProductionOrder?> GetByIdWithDetailsAsync(Guid id);
        Task<IEnumerable<ProductionOrder>> GetAllAsync();
        Task<IEnumerable<ProductionOrder>> GetByStatusAsync(string status);
        Task<string> GetNextOrderNumberAsync();
        Task<ProductionOrder> CreateAsync(ProductionOrder order);
        Task<ProductionOrder> UpdateAsync(ProductionOrder order);
    }
}
