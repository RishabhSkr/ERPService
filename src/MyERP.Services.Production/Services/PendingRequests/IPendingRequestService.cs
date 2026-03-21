/*
 * PendingRequest Service
 */

using MyERP.Services.Production.DTOs.PendingRequest;

namespace MyERP.Services.Production.Services.PendingRequests
{
    public interface IPendingRequestService
    {
        Task<PendingRequestDto> GetByIdAsync(Guid id);
        Task<IEnumerable<PendingRequestDto>> GetAllAsync();
        Task<IEnumerable<PendingRequestDto>> GetPendingAsync();
        Task ApproveAsync(Guid id, ApproveRequestDto dto, Guid? userId = null);
        Task CancelAsync(Guid id, CancelRequestDto dto, Guid? userId = null);
    }
}
