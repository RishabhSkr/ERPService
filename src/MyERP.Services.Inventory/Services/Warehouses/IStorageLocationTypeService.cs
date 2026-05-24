using MyERP.Services.Inventory.DTOs.Warehouse;

namespace MyERP.Services.Inventory.Services.Warehouses
{
    public interface IStorageLocationTypeService
    {
        Task<IEnumerable<StorageLocationTypeDto>> GetAllAsync();
        Task<StorageLocationTypeDto> GetByIdAsync(Guid id);
        Task<StorageLocationTypeDto> CreateAsync(CreateStorageLocationTypeDto dto);
        Task<StorageLocationTypeDto> UpdateAsync(Guid id, UpdateStorageLocationTypeDto dto);
        Task DeleteAsync(Guid id);
    }
}
