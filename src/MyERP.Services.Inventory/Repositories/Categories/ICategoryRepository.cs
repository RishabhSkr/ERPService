using MyERP.Services.Inventory.Models;

namespace MyERP.Services.Inventory.Repositories.Categories
{
    public interface ICategoryRepository
    {
        Task<Category?> GetByIdAsync(Guid id);
        Task<Category?> GetByIdWithRelationsAsync(Guid id);
        Task<Category?> GetByCodeAsync(string code);
        Task<List<Category>> GetAllActiveAsync();
        Task<bool> ExistsAsync(Guid id);
        Task<bool> CodeExistsAsync(string code, Guid? excludeId = null);
        Task<Category> AddAsync(Category category);
        Task UpdateAsync(Category category);
        Task<int> GetProductCountAsync(Guid categoryId);
        Task<int> GetRawMaterialCountAsync(Guid categoryId);
    }
}
