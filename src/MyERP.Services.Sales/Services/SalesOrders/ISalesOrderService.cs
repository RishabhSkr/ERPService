using MyERP.Services.Sales.DTOs;
using MyERP.Services.Sales.DTOs.SalesOrders;

namespace MyERP.Services.Sales.Services.SalesOrders
{
    public interface ISalesOrderService
    {
        Task<SalesOrderResponseDto> CreateAsync(CreateSalesOrderDto dto, Guid? createdBy = null);
        Task<PagedResponse<SalesOrderListDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10, string? status = null, Guid? customerId = null);
        Task<SalesOrderResponseDto> GetByIdAsync(Guid id);
        Task<SalesOrderResponseDto> UpdateStatusAsync(Guid id, UpdateOrderStatusDto dto);
        Task<bool> CancelAsync(Guid id, string? reason = null);
        // Fulfillment
        Task DispatchAsync(Guid orderId, DispatchRequestDto dto);
        Task MarkDeliveredAsync(Guid orderId);
        Task<List<FulfillmentDashboardDto>> GetFulfillmentDashboardAsync();

    }
}
