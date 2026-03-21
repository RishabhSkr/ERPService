using MyERP.Services.Production.Models;

namespace MyERP.Services.Production.Repositories.WorkCenter
{
    public interface IWorkCenterRepository
    {
        Task<Models.WorkCenter?> GetByIdAsync(Guid id);
        Task<Models.WorkCenter?> GetByIdWithEquipmentAsync(Guid id);
        Task<IEnumerable<Models.WorkCenter>> GetAllAsync();
        Task<bool> ExistsByCodeAsync(string centerCode);
        Task<Models.WorkCenter> CreateAsync(Models.WorkCenter entity);
        Task<Models.WorkCenter> UpdateAsync(Models.WorkCenter entity);
        Task DeleteAsync(Guid id);
    }
}
