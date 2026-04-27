using MyERP.Services.Inventory.Models;

namespace MyERP.Services.Inventory.Repositories.Warehouses
{
    public interface IStorageLocationTypeRepository
    {
        Task<IEnumerable<StorageLocationType>> GetAllAsync();
        Task<StorageLocationType?> GetByIdAsync(Guid id);
        Task<StorageLocationType> AddAsync(StorageLocationType type);
        Task UpdateAsync(StorageLocationType type);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<bool> TypeCodeExistsAsync(string code, Guid? excludeId = null);
    }
}
