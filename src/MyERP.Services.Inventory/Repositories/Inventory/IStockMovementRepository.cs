using MyERP.Services.Inventory.Models;

namespace MyERP.Services.Inventory.Repositories.Inventory
{
    public interface IStockMovementRepository
    {
        Task<StockMovement> AddAsync(StockMovement movement);
        Task<(List<StockMovement> Movements, int TotalCount)> GetAllAsync(
            int pageNumber, int pageSize, string? itemType = null, Guid? itemId = null,
            string? movementType = null, DateTime? startDate = null, DateTime? endDate = null);
        Task<StockMovement?> GetByIdAsync(Guid id);
        Task<List<StockMovement>> GetByItemAsync(string itemType, Guid itemId, int limit = 50);
    }
}
