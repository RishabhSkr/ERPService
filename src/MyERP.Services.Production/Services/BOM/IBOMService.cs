/*
 * BOM Service Interface
 * 
 * ðŸ“š INDUSTRY vs NOOB:
 * 
 * âŒ NOOB: Business logic in controllers
 * âœ… INDUSTRY:
 *    1. Service layer contains business logic
 *    2. Controllers only handle HTTP concerns
 *    3. Logic reusable across controllers, background jobs, etc.
 *    4. Easy to unit test without HTTP context
 */

using MyERP.Services.Production.DTOs.BOM;

namespace MyERP.Services.Production.Services.BOM
{
    public interface IBOMService
    {
        Task<BOMDto> GetByIdAsync(Guid bomId);
        Task<BOMDto?> GetActiveByProductIdAsync(Guid productId);
        Task<IEnumerable<BOMDto>> GetAllAsync();
        Task<BOMDto> CreateAsync(CreateBOMDto dto, Guid? userId = null);
        Task<BOMDto> UpdateAsync(Guid bomId, UpdateBOMDto dto);
        Task DeactivateAsync(Guid bomId);
    }
}
