/*
 * BOM Repository Interface
 * 
 * 📚 INDUSTRY vs NOOB:
 * 
 * ❌ NOOB: Access DbContext directly in controllers
 * ✅ INDUSTRY:
 *    1. Repository abstracts data access
 *    2. Interface allows mocking in tests
 *    3. Single Responsibility: only data operations
 *    4. Can add caching layer without changing service
 */

using MyERP.Services.Production.Models;

namespace MyERP.Services.Production.Repositories.BOM
{
    public interface IBOMRepository
    {
        Task<Models.BOM?> GetByIdAsync(Guid bomId);
        Task<Models.BOM?> GetByIdWithLinesAsync(Guid bomId);
        Task<Models.BOM?> GetActiveByProductIdAsync(Guid productId);
        Task<IEnumerable<Models.BOM>> GetAllAsync();
        Task<bool> ExistsByCodeAsync(string bomCode);
        Task<Models.BOM> CreateAsync(Models.BOM bom);
        Task<Models.BOM> UpdateAsync(Models.BOM bom);
        Task DeleteAsync(Guid bomId);
    }
}
