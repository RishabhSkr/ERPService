using MyERP.Services.Production.Constants;
using MyERP.Services.Production.DTOs.WorkOrder;
using MyERP.Services.Production.Exceptions;
using MyERP.Services.Production.Models;
using MyERP.Services.Production.Repositories.Equipment;
using MyERP.Services.Production.Repositories.ProcessRoute;
using MyERP.Services.Production.Repositories.ProductionOrders;
using MyERP.Services.Production.Repositories.WorkOrder;

namespace MyERP.Services.Production.Services.WorkOrder
{
    public class WorkOrderService : IWorkOrderService
    {
        private readonly IWorkOrderRepository _repository;
        private readonly IProductionOrderRepository _poRepository;
        private readonly IProcessRouteRepository _routeRepository;
        private readonly IEquipmentRepository _equipmentRepository;
        private readonly ILogger<WorkOrderService> _logger;

        public WorkOrderService(
            IWorkOrderRepository repository,
            IProductionOrderRepository poRepository,
            IProcessRouteRepository routeRepository,
            IEquipmentRepository equipmentRepository,
            ILogger<WorkOrderService> logger)
        {
            _repository = repository;
            _poRepository = poRepository;
            _routeRepository = routeRepository;
            _equipmentRepository = equipmentRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<WorkOrderDto>> GenerateWorkOrdersAsync(Guid productionOrderId)
        {
            var po = await _poRepository.GetByIdAsync(productionOrderId);
            if (po == null) throw new NotFoundException("ProductionOrder", productionOrderId);

            if (await _repository.ExistForProductionOrderAsync(productionOrderId))
                throw new AppException("Work orders already generated for this production order");

            var route = await _routeRepository.GetActiveByProductIdAsync(po.ProductId);
            if (route == null)
                throw new AppException($"No active ProcessRoute found for product {po.ProductId}");

            var counter = await _repository.GetCountAsync();
            var workOrders = new List<Models.WorkOrder>();

            foreach (var step in route.Steps.OrderBy(s => s.StepNumber))
            {
                counter++;
                workOrders.Add(new Models.WorkOrder
                {
                    WorkOrderId = Guid.NewGuid(),
                    WorkOrderNumber = $"WO-{DateTime.UtcNow:yyyy}-{counter:D4}",
                    ProductionOrderId = productionOrderId,
                    ProcessRouteStepId = step.ProcessRouteStepId,
                    ProcessId = step.ProcessId,
                    WorkCenterId = route.WorkCenterId,
                    StepNumber = step.StepNumber,
                    OperationName = step.Process?.ProcessName ?? "Unknown",
                    QuantityPlanned = po.QuantityPlanned,
                    Status = WorkOrderStatus.Pending
                });
            }

            await _repository.AddRangeAsync(workOrders);
            _logger.LogInformation("Generated {Count} work orders for PO {OrderNumber}",
                workOrders.Count, po.OrderNumber);

            return await GetByProductionOrderAsync(productionOrderId);
        }

        public async Task<IEnumerable<WorkOrderDto>> GetByProductionOrderAsync(Guid productionOrderId)
        {
            var entities = await _repository.GetByProductionOrderAsync(productionOrderId);
            return entities.Select(MapToDto);
        }

        public async Task<WorkOrderExecutionDto> ActivateAsync(Guid workOrderId, ActivateWorkOrderDto dto)
        {
            var wo = await _repository.GetByIdAsync(workOrderId);
            if (wo == null) throw new NotFoundException("WorkOrder", workOrderId);

            if (wo.Status == WorkOrderStatus.Cancelled)
                throw new AppException("Cannot activate cancelled work order");

            var equipment = await _equipmentRepository.GetByIdAsync(dto.EquipmentId);
            if (equipment == null || !equipment.IsActive)
                throw new AppException("Equipment not found or not active");

            if (equipment.Status == EquipmentStatus.Maintenance)
                throw new AppException($"Equipment {equipment.EquipmentCode} is under maintenance");

            if (wo.WorkCenterId.HasValue && equipment.WorkCenterId != wo.WorkCenterId.Value)
                throw new AppException("Equipment does not belong to this work center");

            if (wo.ProcessId.HasValue)
            {
                var canPerform = await _equipmentRepository.CanPerformProcessAsync(dto.EquipmentId, wo.ProcessId.Value);
                if (!canPerform)
                    throw new AppException($"Equipment {equipment.EquipmentCode} cannot perform this process. Please link the process first.");
            }

            var execution = new WorkOrderExecution
            {
                ExecutionId = Guid.NewGuid(),
                WorkOrderId = workOrderId,
                EquipmentId = dto.EquipmentId,
                ActivatedBy = dto.ActivatedBy,
                ActivatedAt = DateTime.UtcNow,
                Status = ExecutionStatus.Active,
                Notes = dto.Notes
            };

            await _repository.CreateExecutionAsync(execution);

            if (wo.Status == WorkOrderStatus.Pending)
            {
                wo.Status = WorkOrderStatus.InProgress;
                await _repository.UpdateAsync(wo);
            }

            _logger.LogInformation("WO {WONumber} activated on {Equipment} by {Operator}",
                wo.WorkOrderNumber, equipment.EquipmentCode, dto.ActivatedBy);

            return MapExecutionToDto(execution, equipment);
        }

        public async Task<WorkOrderExecutionDto> CompleteExecutionAsync(
            Guid workOrderId, Guid executionId, CompleteExecutionDto dto)
        {
            var execution = await _repository.GetExecutionByIdAsync(executionId);
            if (execution == null || execution.WorkOrderId != workOrderId)
                throw new NotFoundException("Execution", executionId);

            if (execution.Status != ExecutionStatus.Active)
                throw new AppException("Execution is not active");

            if (dto.QuantityProduced <= 0)
                throw new AppException("Must report quantity produced");

            execution.QuantityProduced = dto.QuantityProduced;
            execution.QuantityScrap = dto.QuantityScrap;
            execution.DeactivatedAt = DateTime.UtcNow;
            execution.Status = ExecutionStatus.Completed;
            execution.Notes = dto.Notes;
            await _repository.UpdateExecutionAsync(execution);

            // Update WO totals
            var wo = await _repository.GetByIdWithExecutionsAsync(workOrderId);
            if (wo != null)
            {
                wo.QuantityCompleted = wo.Executions.Sum(e => e.QuantityProduced);
                wo.QuantityScrap = wo.Executions.Sum(e => e.QuantityScrap);

                var hasActive = wo.Executions.Any(e => e.Status == ExecutionStatus.Active);
                if (!hasActive && wo.QuantityCompleted >= wo.QuantityPlanned)
                {
                    wo.Status = WorkOrderStatus.Completed;
                    _logger.LogInformation("WO {WO} auto-completed: {Qty}/{Planned}",
                        wo.WorkOrderNumber, wo.QuantityCompleted, wo.QuantityPlanned);
                }
                await _repository.UpdateAsync(wo);
            }

            return MapExecutionToDto(execution, execution.Equipment!);
        }

        public async Task<WorkOrderExecutionDto> PauseExecutionAsync(Guid workOrderId, Guid executionId)
        {
            var execution = await _repository.GetExecutionByIdAsync(executionId);
            if (execution == null || execution.WorkOrderId != workOrderId)
                throw new NotFoundException("Execution", executionId);

            if (execution.Status != ExecutionStatus.Active)
                throw new AppException("Execution is not active");

            execution.Status = ExecutionStatus.Paused;
            execution.DeactivatedAt = DateTime.UtcNow;
            await _repository.UpdateExecutionAsync(execution);

            return MapExecutionToDto(execution, execution.Equipment!);
        }

        public async Task<IEnumerable<WorkOrderExecutionDto>> GetExecutionsAsync(Guid workOrderId)
        {
            var executions = await _repository.GetExecutionsByWorkOrderAsync(workOrderId);
            return executions.Select(e => MapExecutionToDto(e, e.Equipment!));
        }

        public async Task<IEnumerable<WorkOrderExecutionDto>> GetEquipmentExecutionsAsync(Guid equipmentId)
        {
            var executions = await _repository.GetExecutionsByEquipmentAsync(equipmentId);
            return executions.Select(e => MapExecutionToDto(e, e.Equipment!));
        }

        private static WorkOrderDto MapToDto(Models.WorkOrder w) => new()
        {
            WorkOrderId = w.WorkOrderId, WorkOrderNumber = w.WorkOrderNumber,
            ProductionOrderNumber = w.ProductionOrder?.OrderNumber ?? "",
            StepNumber = w.StepNumber, OperationName = w.OperationName,
            ProcessCode = w.Process?.ProcessCode,
            WorkCenterCode = w.WorkCenter?.CenterCode,
            WorkCenterName = w.WorkCenter?.CenterName,
            QuantityPlanned = w.QuantityPlanned,
            QuantityCompleted = w.QuantityCompleted,
            QuantityScrap = w.QuantityScrap, Status = w.Status,
            Executions = w.Executions.Select(e => new WorkOrderExecutionDto
            {
                ExecutionId = e.ExecutionId,
                EquipmentCode = e.Equipment?.EquipmentCode ?? "",
                EquipmentName = e.Equipment?.EquipmentName ?? "",
                ActivatedBy = e.ActivatedBy, ActivatedAt = e.ActivatedAt,
                DeactivatedAt = e.DeactivatedAt,
                QuantityProduced = e.QuantityProduced,
                QuantityScrap = e.QuantityScrap, Status = e.Status, Notes = e.Notes
            }).ToList()
        };

        private static WorkOrderExecutionDto MapExecutionToDto(WorkOrderExecution e, Models.Equipment eq) => new()
        {
            ExecutionId = e.ExecutionId, EquipmentCode = eq.EquipmentCode,
            EquipmentName = eq.EquipmentName, ActivatedBy = e.ActivatedBy,
            ActivatedAt = e.ActivatedAt, DeactivatedAt = e.DeactivatedAt,
            QuantityProduced = e.QuantityProduced, QuantityScrap = e.QuantityScrap,
            Status = e.Status, Notes = e.Notes
        };
    }
}
