using MyERP.Services.Production.Models;

namespace MyERP.Services.Production.Repositories.Equipment
{
    public interface IEquipmentRepository
    {
        Task<Models.Equipment?> GetByIdAsync(Guid id);
        Task<Models.Equipment?> GetByIdWithWorkCenterAsync(Guid id);
        Task<IEnumerable<Models.Equipment>> GetAllAsync();
        Task<IEnumerable<Models.Equipment>> GetByWorkCenterAsync(Guid workCenterId);
        Task<bool> ExistsByCodeAsync(string equipmentCode);
        Task<Models.Equipment> CreateAsync(Models.Equipment entity);
        Task<Models.Equipment> UpdateAsync(Models.Equipment entity);
        Task DeleteAsync(Guid id);

        // EquipmentProcess mapping
        Task<IEnumerable<EquipmentProcess>> GetLinkedProcessesAsync(Guid equipmentId);
        Task ReplaceLinkedProcessesAsync(Guid equipmentId, List<Guid> processIds);
        Task<bool> CanPerformProcessAsync(Guid equipmentId, Guid processId);
    }
}
