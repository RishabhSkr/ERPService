using MyERP.Services.Inventory.Models;

namespace MyERP.Services.Inventory.Repositories.Warehouses
{
    public interface IWarehouseRepository
    {
        Task<Warehouse?> GetByIdAsync(Guid id);
        Task<Warehouse?> GetByIdWithDetailsAsync(Guid id);
        Task<Warehouse?> GetFirstActiveAsync();
        Task<List<Warehouse>> GetAllActiveAsync();
        Task<List<Warehouse>> GetAllAsync();
        Task<bool> ExistsAsync(Guid id);
        Task<Warehouse> AddAsync(Warehouse warehouse);
        Task<Warehouse> UpdateAsync(Warehouse warehouse);
        Task DeleteAsync(Guid id);
    }
}
