using MyERP.Services.Inventory.DTOs.Units;

namespace MyERP.Services.Inventory.Services.Units
{
    public interface IUnitService
    {
        Task<UnitResponseDto> CreateAsync(CreateUnitDto dto);
        Task<List<UnitResponseDto>> GetAllAsync();
        Task<UnitResponseDto> GetByIdAsync(Guid id);
        Task<UnitResponseDto> UpdateAsync(Guid id, CreateUnitDto dto);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> RestoreAsync(Guid id);  // Restore soft-deleted unit
    }
}
