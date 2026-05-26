using MyERP.Services.Production.DTOs.ProcessRoute;

namespace MyERP.Services.Production.Services.ProcessRoute
{
    public interface IProcessRouteService
    {
        Task<ProcessRouteDto> GetByIdAsync(Guid id);
        Task<ProcessRouteDto?> GetByProductIdAsync(Guid productId);
        Task<IEnumerable<ProcessRouteDto>> GetAllAsync();
        Task<ProcessRouteDto> CreateAsync(CreateProcessRouteDto dto);
        Task<ProcessRouteDto> UpdateAsync(Guid id, CreateProcessRouteDto dto);
        Task DeleteAsync(Guid routeId);
    }
}
