/*
 * ProductionOrder Service
 */

using MyERP.Services.Production.DTOs.ProductionOrder;

namespace MyERP.Services.Production.Services.ProductionOrders
{
    public interface IProductionOrderService
    {
        Task<ProductionOrderDto> GetByIdAsync(Guid id);
        Task<IEnumerable<ProductionOrderDto>> GetAllAsync();
        Task<IEnumerable<ProductionOrderDto>> GetByStatusAsync(string status);
        Task<ProductionOrderDto> CreateAsync(CreateProductionOrderDto dto, Guid? userId = null);
        Task StartAsync(Guid id);
        Task UpdateProgressAsync(Guid id, UpdateProgressDto dto);
        Task CompleteAsync(Guid id, CompleteBatchDto dto);
        Task ForceCompleteAsync(Guid id);
        Task CancelAsync(Guid id, string reason);
        Task RetryReservationAsync(Guid id);
        // Release
        Task ReleaseAsync(Guid id, Guid? userId = null);
        
    }
}
