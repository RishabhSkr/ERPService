using MyERP.Services.Inventory.Models;

namespace MyERP.Services.Inventory.Repositories.Units
{
    public interface IUnitRepository
    {
        Task<Unit?> GetByIdAsync(Guid id);
        Task<Unit?> GetByCodeAsync(string code);
        Task<List<Unit>> GetAllActiveAsync();
        Task<bool> ExistsAsync(Guid id);
        Task<bool> CodeExistsAsync(string code, Guid? excludeId = null);
        Task<Unit> AddAsync(Unit unit);
        Task UpdateAsync(Unit unit);
        Task<bool> IsInUseAsync(Guid unitId);
    }
}
