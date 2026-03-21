/*
 * PendingRequest Repository
 */

using MyERP.Services.Production.Models;

namespace MyERP.Services.Production.Repositories.PendingRequests
{
    public interface IPendingRequestRepository
    {
        Task<PendingRequest?> GetByIdAsync(Guid id);
        Task<PendingRequest?> GetByIdWithItemsAsync(Guid id);
        Task<IEnumerable<PendingRequest>> GetAllAsync();
        Task<IEnumerable<PendingRequest>> GetByStatusAsync(string status);
        Task<bool> ExistsBySalesOrderIdAsync(Guid salesOrderId);
        Task<PendingRequest> CreateAsync(PendingRequest request);
        Task<PendingRequest> UpdateAsync(PendingRequest request);
    }
}
