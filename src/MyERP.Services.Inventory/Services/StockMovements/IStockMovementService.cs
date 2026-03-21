using MyERP.Services.Inventory.DTOs;
using MyERP.Services.Inventory.DTOs.StockMovements;

namespace MyERP.Services.Inventory.Services.StockMovements
{
    public interface IStockMovementService
    {
        Task<StockMovementResponseDto> RecordMovementAsync(RecordStockMovementDto dto, Guid? userId = null);
        Task<PagedResponse<StockMovementResponseDto>> GetAllAsync(
            int pageNumber = 1,
            int pageSize = 20,
            string? itemType = null,
            Guid? itemId = null,
            string? movementType = null,
            DateTime? startDate = null,
            DateTime? endDate = null);
        Task<StockMovementResponseDto> GetByIdAsync(Guid id);
        Task<List<StockMovementResponseDto>> GetByItemAsync(string itemType, Guid itemId);
    }
}
