using MyERP.Services.Production.DTOs.WorkOrder;

namespace MyERP.Services.Production.Services.WorkOrder
{
    public interface IWorkOrderService
    {
        Task<IEnumerable<WorkOrderDto>> GenerateWorkOrdersAsync(Guid productionOrderId);
        Task<IEnumerable<WorkOrderDto>> GetByProductionOrderAsync(Guid productionOrderId);
        Task<WorkOrderExecutionDto> ActivateAsync(Guid workOrderId, ActivateWorkOrderDto dto);
        Task<WorkOrderExecutionDto> CompleteExecutionAsync(Guid workOrderId, Guid executionId, CompleteExecutionDto dto);
        Task<WorkOrderExecutionDto> PauseExecutionAsync(Guid workOrderId, Guid executionId);
        Task<IEnumerable<WorkOrderExecutionDto>> GetExecutionsAsync(Guid workOrderId);
        Task<IEnumerable<WorkOrderExecutionDto>> GetEquipmentExecutionsAsync(Guid equipmentId);
    }
}
