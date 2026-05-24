using MyERP.Services.Production.Models;

namespace MyERP.Services.Production.Repositories.WorkOrder
{
    public interface IWorkOrderRepository
    {
        Task<Models.WorkOrder?> GetByIdAsync(Guid id);
        Task<Models.WorkOrder?> GetByIdWithExecutionsAsync(Guid id);
        Task<IEnumerable<Models.WorkOrder>> GetByProductionOrderAsync(Guid productionOrderId);
        Task<int> GetCountAsync();
        Task<bool> ExistForProductionOrderAsync(Guid productionOrderId);
        Task AddAsync(Models.WorkOrder workOrder);
        Task AddRangeAsync(IEnumerable<Models.WorkOrder> workOrders);
        Task<Models.WorkOrder> UpdateAsync(Models.WorkOrder entity);

        /// <summary>
        /// HybridSum: Completed → QuantityCompleted, Active → QuantityPlanned, Cancelled → excluded
        /// Prevents over-planning
        /// </summary>
        Task<decimal> GetHybridSumForStepAsync(Guid productionOrderId, Guid processRouteStepId);

        /// <summary>
        /// Get all WOs for a PO grouped by step (for dashboard/planning)
        /// </summary>
        Task<IEnumerable<Models.WorkOrder>> GetAllByProductionOrderWithDetailsAsync(Guid productionOrderId);

        // Execution tracking
        Task<WorkOrderExecution?> GetExecutionByIdAsync(Guid executionId);
        Task<WorkOrderExecution> CreateExecutionAsync(WorkOrderExecution execution);
        Task<WorkOrderExecution> UpdateExecutionAsync(WorkOrderExecution execution);
        Task<IEnumerable<WorkOrderExecution>> GetExecutionsByWorkOrderAsync(Guid workOrderId);
        Task<IEnumerable<WorkOrderExecution>> GetExecutionsByEquipmentAsync(Guid equipmentId);
    }
}
