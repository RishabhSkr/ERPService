using MyERP.Services.Production.DTOs.Equipment;
using MyERP.Services.Production.DTOs.Process;

namespace MyERP.Services.Production.Services.Equipment
{
    public interface IEquipmentService
    {
        Task<EquipmentDto> GetByIdAsync(Guid id);
        Task<IEnumerable<EquipmentDto>> GetAllAsync();
        Task<IEnumerable<EquipmentDto>> GetByWorkCenterAsync(Guid workCenterId);
        Task<EquipmentDto> CreateAsync(CreateEquipmentDto dto);
        Task<EquipmentDto> UpdateAsync(Guid id, CreateEquipmentDto dto);
        Task DeactivateAsync(Guid id);
        Task LinkProcessesAsync(Guid equipmentId, LinkProcessesDto dto);
        Task<IEnumerable<ProcessDto>> GetLinkedProcessesAsync(Guid equipmentId);
    }
}
