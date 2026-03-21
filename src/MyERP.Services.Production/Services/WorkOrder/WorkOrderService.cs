using MyERP.Services.Production.Constants;
using MyERP.Services.Production.DTOs.WorkOrder;
using MyERP.Services.Production.Events.Publishers;
using MyERP.Services.Production.Exceptions;
using MyERP.Services.Production.Models;
using MyERP.Services.Production.Repositories.BOM;
using MyERP.Services.Production.Repositories.Equipment;
using MyERP.Services.Production.Repositories.ProcessRoute;
using MyERP.Services.Production.Repositories.ProductionOrders;
using MyERP.Services.Production.Repositories.WorkCenter;
using MyERP.Services.Production.Repositories.WorkOrder;
using MyERP.Shared.Events;

namespace MyERP.Services.Production.Services.WorkOrder
{
    public class WorkOrderService : IWorkOrderService
    {
        private readonly IWorkOrderRepository _repository;
        private readonly IProductionOrderRepository _poRepository;
        private readonly IProcessRouteRepository _routeRepository;
        private readonly IEquipmentRepository _equipmentRepository;
        private readonly IWorkCenterRepository _workCenterRepository;
        private readonly IBOMRepository _bomRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILogger<WorkOrderService> _logger;

        public WorkOrderService(
            IWorkOrderRepository repository,
            IProductionOrderRepository poRepository,
            IProcessRouteRepository routeRepository,
            IEquipmentRepository equipmentRepository,
            IWorkCenterRepository workCenterRepository,
            IBOMRepository bomRepository,
            IEventPublisher eventPublisher,
            ILogger<WorkOrderService> logger)
        {
            _repository = repository;
            _poRepository = poRepository;
            _routeRepository = routeRepository;
            _equipmentRepository = equipmentRepository;
            _workCenterRepository = workCenterRepository;
            _bomRepository = bomRepository;
            _eventPublisher = eventPublisher;
            _logger = logger;
        }

        // ====================================================================
        // CREATE WO — User specifies qty + step + workCenter
        // ====================================================================
        public async Task<WorkOrderDto> CreateAsync(CreateWorkOrderDto dto)
        {
            // ---- GUARD CHAIN (6 checks, fail-fast) ----

            // Guard 1: PO exists + not cancelled
            var po = await _poRepository.GetByIdAsync(dto.ProductionOrderId);
            if (po == null)
                throw new NotFoundException("ProductionOrder", dto.ProductionOrderId);
            if (po.Status == ProductionOrderStatus.Cancelled)
                throw new AppException("Cannot create WO for cancelled Production Order");

            // Guard 2: PO must be Released or InProgress
            if (po.Status != ProductionOrderStatus.Released && po.Status != ProductionOrderStatus.InProgress)
                throw new AppException($"PO must be Released or InProgress. Current: '{po.Status}'");

            // Guard 3: ProcessRouteStep exists
            var route = await _routeRepository.GetActiveByProductIdAsync(po.ProductId);
            if (route == null)
                throw new AppException($"No active ProcessRoute for product {po.ProductCode}");

            var step = route.Steps.FirstOrDefault(s => s.ProcessRouteStepId == dto.ProcessRouteStepId);
            if (step == null)
                throw new AppException("ProcessRouteStep not found in this product's route");

            // Guard 4: WorkCenter exists + active
            var workCenter = await _workCenterRepository.GetByIdAsync(dto.WorkCenterId);
            if (workCenter == null)
                throw new NotFoundException("WorkCenter", dto.WorkCenterId);
            if (!workCenter.IsActive)
                throw new AppException($"WorkCenter '{workCenter.CenterCode}' is not active");

            // Guard 5: Qty > 0
            if (dto.QuantityPlanned <= 0)
                throw new AppException("Quantity must be greater than 0");

            // Guard 6: Qty ≤ RemainingQty (HybridSum check)
            var hybridSum = await _repository.GetHybridSumForStepAsync(dto.ProductionOrderId, dto.ProcessRouteStepId);
            var remaining = po.QuantityPlanned - hybridSum;
            if (dto.QuantityPlanned > remaining)
                throw new AppException(
                    $"Quantity {dto.QuantityPlanned} exceeds remaining quantity ({remaining}) for this step. " +
                    $"PO total: {po.QuantityPlanned}, already planned/completed: {hybridSum}");

            // ---- CREATE WO ----
            var counter = await _repository.GetCountAsync();
            var wo = new Models.WorkOrder
            {
                WorkOrderId = Guid.NewGuid(),
                WorkOrderNumber = $"WO-{DateTime.UtcNow:yyyy}-{counter + 1:D4}",
                ProductionOrderId = dto.ProductionOrderId,

                // From route step (auto)
                ProcessRouteStepId = step.ProcessRouteStepId,
                ProcessId = step.ProcessId,
                StepNumber = step.StepNumber,
                OperationName = step.Process?.ProcessName ?? "Unknown",

                // Route version lock
                RouteCode = route.RouteCode,
                RouteVersion = route.Version,

                // From PO (auto)
                ProductCode = po.ProductCode,
                ProductName = po.ProductName,

                // User specified
                WorkCenterId = dto.WorkCenterId,
                QuantityPlanned = dto.QuantityPlanned,
                ScheduledStart = dto.ScheduledStart,
                ScheduledEnd = dto.ScheduledEnd,
                Notes = dto.Notes,

                // Defaults
                Status = WorkOrderStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(wo);

            _logger.LogInformation(
                "Created WO {WONumber}: Step={Step}, Qty={Qty}, Remaining={Remaining}",
                wo.WorkOrderNumber, step.StepNumber, dto.QuantityPlanned, remaining - dto.QuantityPlanned);

            // Return full DTO
            var saved = await _repository.GetByIdWithExecutionsAsync(wo.WorkOrderId);
            return MapToDto(saved!);
        }

        // ====================================================================
        // RELEASE WO — Reserve materials (BOM × WO qty)
        // ====================================================================
        public async Task<WorkOrderDto> ReleaseAsync(Guid workOrderId)
        {
            var wo = await _repository.GetByIdAsync(workOrderId);
            if (wo == null) throw new NotFoundException("WorkOrder", workOrderId);

            if (wo.Status != WorkOrderStatus.Pending)
                throw new AppException($"Can only release Pending WOs. Current: '{wo.Status}'");

            // Set status
            wo.Status = WorkOrderStatus.Released;
            wo.ReservationStatus = ReservationStatus.Pending;
            wo.ReservationAttempts = 1;
            wo.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(wo);

            // PUBLISH: Reserve materials for WO qty only (not full PO qty)
            await PublishWoReservationEvent(wo);

            _logger.LogInformation("Released WO {WONumber}, reservation pending", wo.WorkOrderNumber);

            var saved = await _repository.GetByIdWithExecutionsAsync(wo.WorkOrderId);
            return MapToDto(saved!);
        }

        // ====================================================================
        // RETRY RESERVATION
        // ====================================================================
        public async Task RetryReservationAsync(Guid workOrderId)
        {
            var wo = await _repository.GetByIdAsync(workOrderId);
            if (wo == null) throw new NotFoundException("WorkOrder", workOrderId);

            if (wo.Status != WorkOrderStatus.Released)
                throw new AppException($"Can only retry for Released WOs. Current: '{wo.Status}'");

            if (wo.ReservationStatus != ReservationStatus.Failed)
                throw new AppException($"Can only retry Failed reservations. Current: '{wo.ReservationStatus}'");

            wo.ReservationStatus = ReservationStatus.Pending;
            wo.ReservationFailReason = null;
            wo.ReservationAttempts += 1;
            wo.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(wo);

            // Re-publish reservation event
            await PublishWoReservationEvent(wo);

            _logger.LogInformation("Retrying reservation for WO {WONumber}, attempt #{Attempt}",
                wo.WorkOrderNumber, wo.ReservationAttempts);
        }

        // ====================================================================
        // CANCEL WO — Any state, with reason
        // ====================================================================
        public async Task CancelAsync(Guid workOrderId, string reason)
        {
            var wo = await _repository.GetByIdWithExecutionsAsync(workOrderId);
            if (wo == null) throw new NotFoundException("WorkOrder", workOrderId);

            if (wo.Status == WorkOrderStatus.Completed)
                throw new AppException("Cannot cancel completed work order");

            if (wo.Status == WorkOrderStatus.Cancelled)
                throw new AppException("Work order is already cancelled");

            var wasReserved = wo.ReservationStatus == ReservationStatus.Reserved
                           || wo.Status == WorkOrderStatus.InProgress;

            // Deactivate any active executions
            if (wo.Executions != null)
            {
                foreach (var exec in wo.Executions.Where(e => e.Status == ExecutionStatus.Active))
                {
                    exec.Status = ExecutionStatus.Completed;
                    exec.DeactivatedAt = DateTime.UtcNow;
                    exec.Notes = $"Auto-deactivated: WO cancelled — {reason}";
                    await _repository.UpdateExecutionAsync(exec);
                }
            }

            wo.Status = WorkOrderStatus.Cancelled;
            wo.CancelReason = reason;
            wo.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(wo);

            if (wasReserved)
            {
                // SAGA: Return reserved materials to Inventory
                var po = await _poRepository.GetByIdWithDetailsAsync(wo.ProductionOrderId);
                if (po != null)
                {
                    var returnEvent = new MaterialReturnRequestedEvent
                    {
                        ProductionOrderId = po.Id,
                        ProductionOrderNumber = po.OrderNumber,
                        MaterialsConsumed = po.MaterialRequirements.Select(m => new MaterialConsumed
                        {
                            RawMaterialId = m.RawMaterialId,
                            MaterialCode = m.MaterialCode,
                            QuantityConsumed = 0,
                            QuantityReturned = po.QuantityPlanned > 0
                                ? (m.QuantityRequired / po.QuantityPlanned) * wo.QuantityPlanned
                                : 0,
                            Unit = m.Unit
                        }).ToList()
                    };
                    await _eventPublisher.PublishAsync(returnEvent);
                }

                _logger.LogInformation("WO {WONumber} cancelled — materials returned", wo.WorkOrderNumber);
            }

            _logger.LogInformation("Cancelled WO {WONumber}: {Reason}", wo.WorkOrderNumber, reason);
        }

        // ====================================================================
        // DASHBOARD — POs with per-step WO aggregation
        // ====================================================================
        public async Task<IEnumerable<WorkOrderDashboardDto>> GetDashboardAsync()
        {
            // Get all active POs (Released/InProgress)
            var releasedPOs = await _poRepository.GetByStatusAsync(ProductionOrderStatus.Released);
            var inProgressPOs = await _poRepository.GetByStatusAsync(ProductionOrderStatus.InProgress);
            var allPOs = releasedPOs.Concat(inProgressPOs).ToList();

            var dashboard = new List<WorkOrderDashboardDto>();

            foreach (var po in allPOs)
            {
                var route = await _routeRepository.GetActiveByProductIdAsync(po.ProductId);
                if (route == null) continue;

                var allWOs = await _repository.GetAllByProductionOrderWithDetailsAsync(po.Id);
                var woList = allWOs.ToList();

                var steps = new List<StepSummaryDto>();

                foreach (var step in route.Steps.OrderBy(s => s.StepNumber))
                {
                    var stepWOs = woList.Where(w => w.ProcessRouteStepId == step.ProcessRouteStepId).ToList();
                    var notCancelled = stepWOs.Where(w => w.Status != WorkOrderStatus.Cancelled).ToList();

                    var completed = notCancelled
                        .Where(w => w.Status == WorkOrderStatus.Completed)
                        .Sum(w => w.QuantityCompleted);

                    var hybridSum = notCancelled.Sum(w =>
                        w.Status == WorkOrderStatus.Completed ? w.QuantityCompleted : w.QuantityPlanned);

                    var unplanned = Math.Max(0, po.QuantityPlanned - hybridSum);
                    var progress = po.QuantityPlanned > 0
                        ? Math.Round(completed / po.QuantityPlanned * 100, 1) : 0;

                    var pipeline = notCancelled
                        .Where(w => w.Status != WorkOrderStatus.Completed)
                        .Sum(w => w.QuantityPlanned);

                    string displayStatus;
                    if (pipeline > 0 && completed == 0) displayStatus = "Planned";
                    else if (completed > 0) displayStatus = "In Progress";
                    else displayStatus = "New";

                    steps.Add(new StepSummaryDto
                    {
                        ProcessRouteStepId = step.ProcessRouteStepId,
                        StepNumber = step.StepNumber,
                        ProcessCode = step.Process?.ProcessCode,
                        ProcessName = step.Process?.ProcessName ?? "Unknown",
                        TotalPlanned = hybridSum,
                        TotalCompleted = completed,
                        UnplannedQuantity = unplanned,
                        ProgressPercentage = progress,
                        WoCount = stepWOs.Count,
                        DisplayStatus = displayStatus
                    });
                }

                dashboard.Add(new WorkOrderDashboardDto
                {
                    ProductionOrderId = po.Id,
                    OrderNumber = po.OrderNumber,
                    ProductCode = po.ProductCode,
                    ProductName = po.ProductName,
                    PoQuantityPlanned = po.QuantityPlanned,
                    PoStatus = po.Status,
                    Steps = steps
                });
            }

            return dashboard;
        }

        // ====================================================================
        // PLANNING INFO — Before creating WO (remaining + equipment)
        // ====================================================================
        public async Task<WorkOrderPlanningInfoDto> GetPlanningInfoAsync(Guid productionOrderId)
        {
            var po = await _poRepository.GetByIdAsync(productionOrderId);
            if (po == null) throw new NotFoundException("ProductionOrder", productionOrderId);

            var route = await _routeRepository.GetActiveByProductIdAsync(po.ProductId);
            if (route == null)
                throw new AppException($"No active ProcessRoute for product {po.ProductCode}");

            var allWOs = await _repository.GetAllByProductionOrderWithDetailsAsync(productionOrderId);
            var woList = allWOs.ToList();

            var steps = new List<PlanningStepDto>();

            foreach (var step in route.Steps.OrderBy(s => s.StepNumber))
            {
                var hybridSum = await _repository.GetHybridSumForStepAsync(productionOrderId, step.ProcessRouteStepId);
                var remaining = Math.Max(0, po.QuantityPlanned - hybridSum);

                // Get equipment that can perform this process
                var equipment = await _equipmentRepository.GetByProcessIdAsync(step.ProcessId);
                var linkedEquipment = equipment?.Select(e => new AvailableEquipmentDto
                {
                    EquipmentId = e.EquipmentId,
                    EquipmentCode = e.EquipmentCode,
                    EquipmentName = e.EquipmentName,
                    WorkCenterId = e.WorkCenterId,
                    WorkCenterCode = e.WorkCenter?.CenterCode ?? "",
                    Status = e.Status,
                    CostPerHour = e.CostPerHour
                }).ToList() ?? new List<AvailableEquipmentDto>();

                // Existing WOs for this step
                var stepWOs = woList
                    .Where(w => w.ProcessRouteStepId == step.ProcessRouteStepId)
                    .Select(w => new ExistingWorkOrderDto
                    {
                        WorkOrderId = w.WorkOrderId,
                        WorkOrderNumber = w.WorkOrderNumber,
                        QuantityPlanned = w.QuantityPlanned,
                        QuantityCompleted = w.QuantityCompleted,
                        Status = w.Status,
                        WorkCenterCode = w.WorkCenter?.CenterCode
                    }).ToList();

                steps.Add(new PlanningStepDto
                {
                    ProcessRouteStepId = step.ProcessRouteStepId,
                    StepNumber = step.StepNumber,
                    ProcessCode = step.Process?.ProcessCode,
                    ProcessName = step.Process?.ProcessName ?? "Unknown",
                    RemainingQuantity = remaining,
                    SetupTimeMinutes = step.SetupTimeMinutes,
                    RunTimePerUnitMinutes = step.RunTimePerUnitMinutes,
                    LinkedEquipment = linkedEquipment,
                    ExistingWorkOrders = stepWOs
                });
            }

            return new WorkOrderPlanningInfoDto
            {
                ProductionOrderId = po.Id,
                OrderNumber = po.OrderNumber,
                ProductCode = po.ProductCode,
                ProductName = po.ProductName,
                PoQuantityPlanned = po.QuantityPlanned,
                RouteCode = route.RouteCode,
                RouteVersion = route.Version,
                Steps = steps
            };
        }

        // ====================================================================
        // EXISTING: Auto-generate (convenience — kept as-is)
        // ====================================================================
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
                    RouteCode = route.RouteCode,
                    RouteVersion = route.Version,
                    ProductCode = po.ProductCode,
                    ProductName = po.ProductName,
                    QuantityPlanned = po.QuantityPlanned,
                    Status = WorkOrderStatus.Pending
                });
            }

            await _repository.AddRangeAsync(workOrders);
            _logger.LogInformation("Generated {Count} work orders for PO {OrderNumber}",
                workOrders.Count, po.OrderNumber);

            return await GetByProductionOrderAsync(productionOrderId);
        }

        // ====================================================================
        // EXISTING: Get WOs, Activate, Complete, Pause, Executions
        // ====================================================================
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

            if (wo.Status == WorkOrderStatus.Completed)
                throw new AppException("Cannot activate completed work order");

            // Must be Released with materials reserved
            if (wo.Status == WorkOrderStatus.Released && wo.ReservationStatus != ReservationStatus.Reserved)
                throw new AppException($"Materials not reserved yet. ReservationStatus: '{wo.ReservationStatus}'");

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

            // Transition: Pending/Released → InProgress on first activation
            if (wo.Status == WorkOrderStatus.Pending || wo.Status == WorkOrderStatus.Released)
            {
                wo.Status = WorkOrderStatus.InProgress;
                wo.ActualStartDate = DateTime.UtcNow;
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
                    wo.ActualEndDate = DateTime.UtcNow;
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

        // ====================================================================
        // MAPPING
        // ====================================================================
        private static WorkOrderDto MapToDto(Models.WorkOrder w) => new()
        {
            WorkOrderId = w.WorkOrderId,
            WorkOrderNumber = w.WorkOrderNumber,
            ProductionOrderId = w.ProductionOrderId,
            ProductionOrderNumber = w.ProductionOrder?.OrderNumber ?? "",
            ProductCode = w.ProductCode,
            ProductName = w.ProductName,
            StepNumber = w.StepNumber,
            OperationName = w.OperationName,
            ProcessCode = w.Process?.ProcessCode,
            RouteCode = w.RouteCode,
            RouteVersion = w.RouteVersion,
            WorkCenterId = w.WorkCenterId,
            WorkCenterCode = w.WorkCenter?.CenterCode,
            WorkCenterName = w.WorkCenter?.CenterName,
            QuantityPlanned = w.QuantityPlanned,
            QuantityCompleted = w.QuantityCompleted,
            QuantityScrap = w.QuantityScrap,
            Status = w.Status,
            ReservationStatus = w.ReservationStatus,
            ReservationFailReason = w.ReservationFailReason,
            ScheduledStart = w.ScheduledStart,
            ScheduledEnd = w.ScheduledEnd,
            ActualStartDate = w.ActualStartDate,
            ActualEndDate = w.ActualEndDate,
            CancelReason = w.CancelReason,
            CreatedBy = w.CreatedBy,
            StartedBy = w.StartedBy,
            CompletedBy = w.CompletedBy,
            CreatedAt = w.CreatedAt,
            Notes = w.Notes,
            Executions = w.Executions?.Select(e => new WorkOrderExecutionDto
            {
                ExecutionId = e.ExecutionId,
                EquipmentCode = e.Equipment?.EquipmentCode ?? "",
                EquipmentName = e.Equipment?.EquipmentName ?? "",
                ActivatedBy = e.ActivatedBy,
                ActivatedAt = e.ActivatedAt,
                DeactivatedAt = e.DeactivatedAt,
                QuantityProduced = e.QuantityProduced,
                QuantityScrap = e.QuantityScrap,
                Status = e.Status,
                Notes = e.Notes
            }).ToList() ?? new()
        };

        private static WorkOrderExecutionDto MapExecutionToDto(WorkOrderExecution e, Models.Equipment eq) => new()
        {
            ExecutionId = e.ExecutionId,
            EquipmentCode = eq.EquipmentCode,
            EquipmentName = eq.EquipmentName,
            ActivatedBy = e.ActivatedBy,
            ActivatedAt = e.ActivatedAt,
            DeactivatedAt = e.DeactivatedAt,
            QuantityProduced = e.QuantityProduced,
            QuantityScrap = e.QuantityScrap,
            Status = e.Status,
            Notes = e.Notes
        };

        // ====================================================================
        // HELPER: Publish reservation event for WO qty
        // ====================================================================
        /// <summary>
        /// Calculates materials proportionally: BOM materials × (WO qty / PO qty)
        /// Example: BOM needs 500kg steel for 100 chairs, WO is for 30 chairs
        ///   → Reserve: (500/100) × 30 = 150kg steel
        /// </summary>
        private async Task PublishWoReservationEvent(Models.WorkOrder wo)
        {
            var po = await _poRepository.GetByIdWithDetailsAsync(wo.ProductionOrderId);
            if (po == null) return;

            // Calculate proportional materials
            var materials = po.MaterialRequirements.Select(m => new MaterialToReserve
            {
                RawMaterialId = m.RawMaterialId,
                MaterialCode = m.MaterialCode,
                // Proportional: (material per unit) × WO qty
                Quantity = po.QuantityPlanned > 0
                    ? (m.QuantityRequired / po.QuantityPlanned) * wo.QuantityPlanned
                    : 0,
                Unit = m.Unit
            }).ToList();

            var reservationEvent = new MaterialReservationRequestedEvent
            {
                ProductionOrderId = po.Id,
                ProductionOrderNumber = po.OrderNumber,
                WorkOrderId = wo.WorkOrderId,         // KEY: WO-level!
                WorkOrderNumber = wo.WorkOrderNumber,
                BomCode = po.BomCode,
                BomVersion = po.BomVersion,
                Materials = materials
            };

            await _eventPublisher.PublishAsync(reservationEvent);

            _logger.LogInformation(
                "Published reservation for WO {WONumber}: {Count} materials, WO qty={Qty}",
                wo.WorkOrderNumber, materials.Count, wo.QuantityPlanned);
        }
    }
}
