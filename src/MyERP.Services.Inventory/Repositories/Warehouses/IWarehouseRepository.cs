using MyERP.Services.Inventory.Models;

namespace MyERP.Services.Inventory.Repositories.Warehouses
{
    public interface IWarehouseRepository
    {
        Task<Warehouse?> GetByIdAsync(Guid id);
        Task<Warehouse?> GetFirstActiveAsync();
        Task<List<Warehouse>> GetAllActiveAsync();
        Task<bool> ExistsAsync(Guid id);
    }
}
