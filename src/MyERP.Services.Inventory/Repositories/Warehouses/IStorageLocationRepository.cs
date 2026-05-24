using MyERP.Services.Inventory.Models;

namespace MyERP.Services.Inventory.Repositories.Warehouses
{
    public interface IStorageLocationRepository
    {
        Task<StorageLocation?> GetByIdAsync(Guid id);
        Task<IEnumerable<StorageLocation>> GetByWarehouseIdAsync(Guid warehouseId);
        Task<IEnumerable<StorageLocation>> GetAllAsync();
        Task<StorageLocation> AddAsync(StorageLocation location);
        Task UpdateAsync(StorageLocation location);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<bool> LocationCodeExistsAsync(string locationCode, Guid warehouseId, Guid? excludeId = null);
    }
}
