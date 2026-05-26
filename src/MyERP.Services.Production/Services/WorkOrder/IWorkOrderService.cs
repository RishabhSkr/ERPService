using MyERP.Services.Production.DTOs.WorkOrder;

namespace MyERP.Services.Production.Services.WorkOrder
{
    public interface IWorkOrderService
    {
        // === NEW: User-controlled WO creation ===
        Task<WorkOrderDto> CreateAsync(CreateWorkOrderDto dto);
        Task<WorkOrderDto> ReleaseAsync(Guid workOrderId);
        Task ForceCompleteAsync(Guid workOrderId);
        Task CancelAsync(Guid workOrderId, string reason);
        Task RetryReservationAsync(Guid workOrderId);
        
        // === NEW: Dashboard + Planning ===
        Task<IEnumerable<WorkOrderDashboardDto>> GetDashboardAsync();
        Task<WorkOrderPlanningInfoDto> GetPlanningInfoAsync(Guid productionOrderId);

        // === EXISTING: Auto-generate (kept as convenience) ===
        Task<IEnumerable<WorkOrderDto>> GenerateWorkOrdersAsync(Guid productionOrderId);
        Task<IEnumerable<WorkOrderDto>> GenerateWorkOrdersForRouteAsync(GenerateRouteWorkOrdersDto dto);
        Task<IEnumerable<WorkOrderDto>> GetByProductionOrderAsync(Guid productionOrderId);

        // === EXISTING: Equipment activation + tracking ===
        Task<WorkOrderExecutionDto> ActivateAsync(Guid workOrderId, ActivateWorkOrderDto dto);
        Task<WorkOrderExecutionDto> CompleteExecutionAsync(Guid workOrderId, Guid executionId, CompleteExecutionDto dto);
        Task<WorkOrderExecutionDto> PauseExecutionAsync(Guid workOrderId, Guid executionId);
        Task<IEnumerable<WorkOrderExecutionDto>> GetExecutionsAsync(Guid workOrderId);
        Task<IEnumerable<WorkOrderExecutionDto>> GetEquipmentExecutionsAsync(Guid equipmentId);
    }
}
