using MyERP.Services.Inventory.DTOs;
using MyERP.Services.Inventory.DTOs.RawMaterials;

namespace MyERP.Services.Inventory.Services.RawMaterials
{
    public interface IRawMaterialService
    {
        Task<RawMaterialResponseDto> CreateAsync(CreateRawMaterialDto dto);
        Task<PagedResponse<RawMaterialListDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10, Guid? categoryId = null, string? searchKeyword = null);
        Task<RawMaterialResponseDto> GetByIdAsync(Guid id);
        Task<RawMaterialResponseDto> UpdateAsync(Guid id, UpdateRawMaterialDto dto);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> RestoreAsync(Guid id);  // Restore soft-deleted raw material
        Task<ReservationResponseDto> ReserveMaterialsAsync(ReserveRawMaterialsDto dto);
        Task<ReservationResponseDto> ReleaseMaterialsAsync(ReleaseRawMaterialsDto dto);
        Task<bool> AddStockAsync(Guid rawMaterialId, Guid warehouseId, decimal quantity, string? batchNumber = null);
    }
}
