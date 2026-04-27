using MyERP.Services.Inventory.DTOs.Warehouse;

namespace MyERP.Services.Inventory.Services.Warehouses
{
    public interface IStorageLocationService
    {
        Task<StorageLocationDto> GetByIdAsync(Guid id);
        Task<IEnumerable<StorageLocationDto>> GetByWarehouseIdAsync(Guid warehouseId);
        Task<IEnumerable<StorageLocationDto>> GetAllAsync();
        Task<StorageLocationDto> CreateAsync(CreateStorageLocationDto dto);
        Task<StorageLocationDto> UpdateAsync(Guid id, UpdateStorageLocationDto dto);
        Task DeleteAsync(Guid id);
    }
}
