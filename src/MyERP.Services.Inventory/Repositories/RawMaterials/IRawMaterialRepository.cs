using MyERP.Services.Inventory.Models;

namespace MyERP.Services.Inventory.Repositories.RawMaterials
{
    public interface IRawMaterialRepository
    {
        Task<RawMaterial?> GetByIdAsync(Guid id);
        Task<RawMaterial?> GetByIdWithInventoryAsync(Guid id);
        Task<RawMaterial?> GetByCodeAsync(string code);
        Task<(List<RawMaterial> Materials, int TotalCount)> GetAllAsync(
            int pageNumber, int pageSize, Guid? categoryId = null, string? searchKeyword = null);
        Task<bool> ExistsAsync(Guid id);
        Task<bool> CodeExistsAsync(string code);
        Task<RawMaterial> AddAsync(RawMaterial material);
        Task UpdateAsync(RawMaterial material);
        Task<RawMaterialInventory?> GetInventoryAsync(Guid rawMaterialId, Guid storageLocationId, string? batchNumber = null);
        Task<List<RawMaterialInventory>> GetInventoriesAsync(Guid rawMaterialId);
        Task<RawMaterialInventory> AddInventoryAsync(RawMaterialInventory inventory);
        Task UpdateInventoryAsync(RawMaterialInventory inventory);
        Task SaveChangesAsync();
    }
}
