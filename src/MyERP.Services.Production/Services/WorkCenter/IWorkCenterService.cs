using MyERP.Services.Production.DTOs.WorkCenter;

namespace MyERP.Services.Production.Services.WorkCenter
{
    public interface IWorkCenterService
    {
        Task<WorkCenterDto> GetByIdAsync(Guid id);
        Task<IEnumerable<WorkCenterDto>> GetAllAsync();
        Task<WorkCenterDto> CreateAsync(CreateWorkCenterDto dto);
        Task<WorkCenterDto> UpdateAsync(Guid id, CreateWorkCenterDto dto);
        Task DeactivateAsync(Guid id);
    }
}
