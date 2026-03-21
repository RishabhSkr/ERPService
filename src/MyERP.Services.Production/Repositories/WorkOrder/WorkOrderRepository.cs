using Microsoft.EntityFrameworkCore;
using MyERP.Services.Production.Constants;
using MyERP.Services.Production.Data;
using MyERP.Services.Production.Models;

namespace MyERP.Services.Production.Repositories.WorkOrder
{
    public class WorkOrderRepository : IWorkOrderRepository
    {
        private readonly ProductionDbContext _context;
        public WorkOrderRepository(ProductionDbContext context) => _context = context;

        public async Task<Models.WorkOrder?> GetByIdAsync(Guid id) =>
            await _context.WorkOrders.FindAsync(id);

        public async Task<Models.WorkOrder?> GetByIdWithExecutionsAsync(Guid id) =>
            await _context.WorkOrders
                .Include(w => w.Executions).ThenInclude(e => e.Equipment)
                .Include(w => w.WorkCenter)
                .Include(w => w.Process)
                .FirstOrDefaultAsync(w => w.WorkOrderId == id);

        public async Task<IEnumerable<Models.WorkOrder>> GetByProductionOrderAsync(Guid productionOrderId) =>
            await _context.WorkOrders
                .Where(w => w.ProductionOrderId == productionOrderId)
                .Include(w => w.ProductionOrder)
                .Include(w => w.Process)
                .Include(w => w.WorkCenter)
                .Include(w => w.Executions).ThenInclude(e => e.Equipment)
                .OrderBy(w => w.StepNumber)
                .ThenBy(w => w.CreatedAt)
                .ToListAsync();

        public async Task<IEnumerable<Models.WorkOrder>> GetAllByProductionOrderWithDetailsAsync(Guid productionOrderId) =>
            await _context.WorkOrders
                .Where(w => w.ProductionOrderId == productionOrderId)
                .Include(w => w.ProductionOrder)
                .Include(w => w.Process)
                .Include(w => w.WorkCenter)
                .OrderBy(w => w.StepNumber)
                .ThenBy(w => w.CreatedAt)
                .ToListAsync();

        public async Task<int> GetCountAsync() =>
            await _context.WorkOrders.CountAsync();

        public async Task<bool> ExistForProductionOrderAsync(Guid productionOrderId) =>
            await _context.WorkOrders.AnyAsync(w => w.ProductionOrderId == productionOrderId);

        /// <summary>
        /// HybridSum: THE critical formula for over-planning prevention.
        /// 
        /// Completed WO → use QuantityCompleted (actual output, scrap excluded)
        /// Active WO     → use QuantityPlanned  (blocks room, prevents over-planning)
        /// Cancelled      → excluded from sum entirely
        /// 
        /// Example: PO=300, WO#1: Planned=200, Completed=180 (scrap=20)
        ///   Plain sum:   remaining = 300 - 200 = 100 → lost 20 can't re-plan ❌
        ///   Hybrid sum:  remaining = 300 - 180 = 120 → 20 freed for re-planning ✅
        /// </summary>
        public async Task<decimal> GetHybridSumForStepAsync(Guid productionOrderId, Guid processRouteStepId)
        {
            return await _context.WorkOrders
                .Where(wo => wo.ProductionOrderId == productionOrderId
                          && wo.ProcessRouteStepId == processRouteStepId
                          && wo.Status != WorkOrderStatus.Cancelled)
                .SumAsync(wo =>
                    wo.Status == WorkOrderStatus.Completed
                        ? wo.QuantityCompleted   // actual output
                        : wo.QuantityPlanned);   // blocks room
        }

        public async Task AddAsync(Models.WorkOrder workOrder)
        {
            _context.WorkOrders.Add(workOrder);
            await _context.SaveChangesAsync();
        }

        public async Task AddRangeAsync(IEnumerable<Models.WorkOrder> workOrders)
        {
            _context.WorkOrders.AddRange(workOrders);
            await _context.SaveChangesAsync();
        }

        public async Task<Models.WorkOrder> UpdateAsync(Models.WorkOrder entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            _context.WorkOrders.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        // Execution tracking
        public async Task<WorkOrderExecution?> GetExecutionByIdAsync(Guid executionId) =>
            await _context.WorkOrderExecutions
                .Include(e => e.Equipment)
                .FirstOrDefaultAsync(e => e.ExecutionId == executionId);

        public async Task<WorkOrderExecution> CreateExecutionAsync(WorkOrderExecution execution)
        {
            _context.WorkOrderExecutions.Add(execution);
            await _context.SaveChangesAsync();
            return execution;
        }

        public async Task<WorkOrderExecution> UpdateExecutionAsync(WorkOrderExecution execution)
        {
            _context.WorkOrderExecutions.Update(execution);
            await _context.SaveChangesAsync();
            return execution;
        }

        public async Task<IEnumerable<WorkOrderExecution>> GetExecutionsByWorkOrderAsync(Guid workOrderId) =>
            await _context.WorkOrderExecutions
                .Include(e => e.Equipment)
                .Where(e => e.WorkOrderId == workOrderId)
                .OrderByDescending(e => e.ActivatedAt).ToListAsync();

        public async Task<IEnumerable<WorkOrderExecution>> GetExecutionsByEquipmentAsync(Guid equipmentId) =>
            await _context.WorkOrderExecutions
                .Include(e => e.Equipment)
                .Where(e => e.EquipmentId == equipmentId)
                .OrderByDescending(e => e.ActivatedAt).ToListAsync();
    }
}
