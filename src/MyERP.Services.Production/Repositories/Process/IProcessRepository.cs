using MyERP.Services.Production.Models;

namespace MyERP.Services.Production.Repositories.Process
{
    public interface IProcessRepository
    {
        Task<Models.Process?> GetByIdAsync(Guid id);
        Task<IEnumerable<Models.Process>> GetAllAsync();
        Task<bool> ExistsByCodeAsync(string processCode);
        Task<Models.Process> CreateAsync(Models.Process entity);
        Task<Models.Process> UpdateAsync(Models.Process entity);
        Task DeleteAsync(Guid id);
    }
}
