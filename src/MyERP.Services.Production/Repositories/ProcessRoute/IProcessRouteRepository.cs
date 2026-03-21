using MyERP.Services.Production.Models;

namespace MyERP.Services.Production.Repositories.ProcessRoute
{
    public interface IProcessRouteRepository
    {
        Task<Models.ProcessRoute?> GetByIdAsync(Guid id);
        Task<Models.ProcessRoute?> GetByIdWithDetailsAsync(Guid id);
        Task<Models.ProcessRoute?> GetActiveByProductIdAsync(Guid productId);
        Task<IEnumerable<Models.ProcessRoute>> GetAllAsync();
        Task<bool> ExistsByCodeAsync(string routeCode);
        Task<Models.ProcessRoute> CreateAsync(Models.ProcessRoute entity);
        Task<Models.ProcessRoute> UpdateAsync(Models.ProcessRoute entity);
        Task ClearStepsAsync(Models.ProcessRoute route);
    }
}
