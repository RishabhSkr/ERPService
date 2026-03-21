using MyERP.Services.Production.DTOs.Process;

namespace MyERP.Services.Production.Services.Process
{
    public interface IProcessService
    {
        Task<ProcessDto> GetByIdAsync(Guid id);
        Task<IEnumerable<ProcessDto>> GetAllAsync();
        Task<ProcessDto> CreateAsync(CreateProcessDto dto);
        Task<ProcessDto> UpdateAsync(Guid id, CreateProcessDto dto);
        Task DeactivateAsync(Guid id);
    }
}
