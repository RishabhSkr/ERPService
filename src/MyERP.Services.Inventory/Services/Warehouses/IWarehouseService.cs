using MyERP.Services.Inventory.DTOs;

namespace MyERP.Services.Inventory.Services.Warehouses
{
    public interface IWarehouseService
    {
        Task<IEnumerable<WarehouseDto>> GetAllAsync();
        Task<WarehouseDto> GetByIdAsync(Guid id);
        Task<WarehouseDto> CreateAsync(CreateWarehouseDto dto);
        Task<WarehouseDto> UpdateAsync(Guid id, UpdateWarehouseDto dto);
        Task DeleteAsync(Guid id);
    }
}
