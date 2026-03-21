/*
 * ProductionOrder Service Implementation
 */

using MyERP.Services.Production.Constants;
using MyERP.Services.Production.DTOs.ProductionOrder;
using MyERP.Services.Production.Events;
using MyERP.Services.Production.Events.Publishers;
using MyERP.Services.Production.Exceptions;
using MyERP.Services.Production.Models;
using MyERP.Services.Production.Repositories.BOM;
using MyERP.Services.Production.Repositories.ProductionOrders;
using MyERP.Shared.Events;


namespace MyERP.Services.Production.Services.ProductionOrders
{
    public class ProductionOrderService : IProductionOrderService
    {
        private readonly IProductionOrderRepository _repository;
        private readonly IBOMRepository _bomRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILogger<ProductionOrderService> _logger;

        public ProductionOrderService(
            IProductionOrderRepository repository,
            IBOMRepository bomRepository,
            IEventPublisher eventPublisher,
            ILogger<ProductionOrderService> logger)
        {
            _repository = repository;
            _bomRepository = bomRepository;
            _eventPublisher = eventPublisher;
            _logger = logger;
        }

        public async Task<ProductionOrderDto> GetByIdAsync(Guid id)
        {
            var order = await _repository.GetByIdWithDetailsAsync(id);
            if (order == null)
                throw new NotFoundException("ProductionOrder", id);
            
            return MapToDto(order);
        }

        public async Task<IEnumerable<ProductionOrderDto>> GetAllAsync()
        {
            var orders = await _repository.GetAllAsync();
            return orders.Select(MapToDto);
        }

        public async Task<IEnumerable<ProductionOrderDto>> GetByStatusAsync(string status)
        {
            var orders = await _repository.GetByStatusAsync(status);
            return orders.Select(MapToDto);
        }

        public async Task<ProductionOrderDto> CreateAsync(CreateProductionOrderDto dto, Guid? userId = null)
        {
            // Verify BOM exists
            var bom = await _bomRepository.GetByIdWithLinesAsync(dto.BOMId);
            if (bom == null)
                throw new NotFoundException("BOM", dto.BOMId);

            if (!bom.IsActive)
                throw new BusinessRuleException("Cannot use inactive BOM");

            // Calculate material requirements
            var materialRequirements = bom.Lines.Select(line => new MaterialRequirement
            {
                Id = Guid.NewGuid(),
                RawMaterialId = line.RawMaterialId,
                MaterialCode = line.MaterialCode,
                MaterialName = line.MaterialName,
                QuantityRequired = line.Quantity * dto.QuantityPlanned * (1 + line.ScrapPercentage / 100),
                Unit = line.Unit,
                Status = MaterialRequirementStatus.Pending
            }).ToList();

            var orderNumber = await _repository.GetNextOrderNumberAsync();
            var order = new ProductionOrder
            {
                Id = Guid.NewGuid(),
                OrderNumber = orderNumber,
                ProductId = dto.ProductId,
                ProductCode = dto.ProductCode,
                ProductName = dto.ProductName,
                BOMId = dto.BOMId,
                QuantityPlanned = dto.QuantityPlanned,
                PlannedStartDate = dto.PlannedStartDate,
                PlannedEndDate = dto.PlannedEndDate,
                Status = ProductionOrderStatus.Create,
                Priority = dto.Priority,
                Notes = dto.Notes,
                CreatedBy = userId,
                MaterialRequirements = materialRequirements
            };

            await _repository.CreateAsync(order);
            
            _logger.LogInformation("Created manual ProductionOrder {OrderNumber}", orderNumber);
            
            return MapToDto(order);
        }
        /// <summary>
        /// Release production order — Sub-State pattern
        /// Sets Status=Released + ReservationStatus=Pending
        /// Publishes MaterialReservationRequestedEvent to Inventory
        /// </summary>
        public async Task ReleaseAsync(Guid id, Guid? userId = null)
        {
            // STEP 1: Get order WITH material details
            var order = await _repository.GetByIdWithDetailsAsync(id);
            if (order == null)
                throw new NotFoundException("ProductionOrder", id);

            // STEP 2: Status validation
            if (order.Status != ProductionOrderStatus.Create)
                throw new BusinessRuleException(
                    $"Cannot release order with status '{order.Status}'. Must be 'Create'.");

            // STEP 3: Material requirements exist?
            if (!order.MaterialRequirements.Any())
                throw new BusinessRuleException(
                    "Cannot release order without material requirements. " +
                    "BOM might be empty or materials not calculated.");

            // STEP 4: BOM still active?
            var bom = await _bomRepository.GetByIdAsync(order.BOMId);
            if (bom == null)
                throw new NotFoundException("BOM", order.BOMId);

            if (!bom.IsActive)
                throw new BusinessRuleException("Cannot release order with inactive BOM");

            // STEP 5: Lock BOM version (Monolith best practice)
            order.BomCode = bom.BomCode;
            order.BomVersion = bom.Version;

            // STEP 6: Set status + sub-state
            order.Status = ProductionOrderStatus.Released;
            order.ReservationStatus = ReservationStatus.Pending;
            order.ReleasedAt = DateTime.UtcNow;
            order.ReleasedBy = userId;
            order.ReservationAttempts = 1;
            order.LastReservationAttempt = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(order);

            // STEP 7: Publish event to Inventory Service
            await PublishReservationEvent(order, bom);

            _logger.LogInformation(
                "Released ProductionOrder {OrderNumber}, BOM {BomCode} v{Version}, reservation pending",
                order.OrderNumber, bom.BomCode, bom.Version);
        }

        /// <summary>
        /// Retry failed reservation — re-publish event to Inventory
        /// Only works when ReservationStatus == Failed
        /// </summary>
        public async Task RetryReservationAsync(Guid id)
        {
            var order = await _repository.GetByIdWithDetailsAsync(id);
            if (order == null)
                throw new NotFoundException("ProductionOrder", id);

            // Can only retry from Released + Failed state
            if (order.Status != ProductionOrderStatus.Released)
                throw new BusinessRuleException(
                    $"Cannot retry reservation for order with status '{order.Status}'. Must be 'Released'.");

            if (order.ReservationStatus != ReservationStatus.Failed)
                throw new BusinessRuleException(
                    $"Cannot retry reservation with status '{order.ReservationStatus}'. Must be 'Failed'.");

            // Reset sub-state to Pending
            order.ReservationStatus = ReservationStatus.Pending;
            order.ReservationFailReason = null;
            order.ReservationAttempts += 1;
            order.LastReservationAttempt = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(order);

            // Re-publish reservation event
            var bom = await _bomRepository.GetByIdAsync(order.BOMId);
            await PublishReservationEvent(order, bom);

            _logger.LogInformation(
                "Retrying reservation for {OrderNumber}, attempt #{Attempt}",
                order.OrderNumber, order.ReservationAttempts);
        }

        /// <summary>
        /// Start production — only when materials are Reserved
        /// </summary>
        public async Task StartAsync(Guid id)
        {
            var order = await _repository.GetByIdAsync(id);
            if (order == null)
                throw new NotFoundException("ProductionOrder", id);

            if (order.Status != ProductionOrderStatus.Released)
                throw new BusinessRuleException(
                    $"Cannot start order with status '{order.Status}'. Must be 'Released'.");

            // Sub-state check: materials must be reserved!
            if (order.ReservationStatus != ReservationStatus.Reserved)
                throw new BusinessRuleException(
                    $"Cannot start production — materials not reserved yet. " +
                    $"Reservation status: '{order.ReservationStatus}'.");

            order.Status = ProductionOrderStatus.InProgress;
            order.ActualStartDate = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(order);

            _logger.LogInformation("Started ProductionOrder {OrderNumber}", order.OrderNumber);
        }

        /// <summary>
        /// Update progress (IoT/machine updates)
        /// </summary>
        public async Task UpdateProgressAsync(Guid id, UpdateProgressDto dto)
        {
            var order = await _repository.GetByIdAsync(id);
            if (order == null)
                throw new NotFoundException("ProductionOrder", id);

            if (order.Status != ProductionOrderStatus.InProgress)
                throw new BusinessRuleException(
                    $"Cannot update progress for order with status '{order.Status}'");

            order.QuantityGood = dto.QuantityCompleted;
            order.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(order);

            // Publish progress event for dashboard
            var progressEvent = new BatchProgressEvent
            {
                ProductionOrderId = order.Id,
                ProductionOrderNumber = order.OrderNumber,
                QuantityCompleted = dto.QuantityCompleted,
                QuantityPlanned = order.QuantityPlanned
            };
            await _eventPublisher.PublishAsync(progressEvent);

            _logger.LogInformation(
                "Updated progress for {OrderNumber}: {Completed}/{Planned}",
                order.OrderNumber, dto.QuantityCompleted, order.QuantityPlanned);
        }

        /// <summary>
        /// Complete production order
        /// </summary>
        public async Task CompleteAsync(Guid id, CompleteBatchDto dto)
        {
            var order = await _repository.GetByIdWithDetailsAsync(id);
            if (order == null)
                throw new NotFoundException("ProductionOrder", id);

            if (order.Status != ProductionOrderStatus.InProgress)
                throw new BusinessRuleException(
                    $"Cannot complete order with status '{order.Status}'");

            // Update quantities
            order.QuantityGood = dto.QuantityGood;
            order.QuantityScrap = dto.QuantityScrap;
            order.Status = ProductionOrderStatus.Completed;
            order.ActualEndDate = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;

            // Update material consumption if provided
            if (dto.MaterialsConsumed != null)
            {
                foreach (var consumed in dto.MaterialsConsumed)
                {
                    var req = order.MaterialRequirements
                        .FirstOrDefault(r => r.RawMaterialId == consumed.RawMaterialId);
                    if (req != null)
                    {
                        req.QuantityConsumed = consumed.QuantityConsumed;
                        req.Status = "Consumed";
                    }
                }
            }

            await _repository.UpdateAsync(order);

            // Publish completion event for Inventory
            var completionEvent = new BatchConcludedEvent
            {
                ProductionOrderId = order.Id,
                ProductionOrderNumber = order.OrderNumber,
                ProductId = order.ProductId,
                ProductCode = order.ProductCode,
                QuantityGood = dto.QuantityGood,
                QuantityScrap = dto.QuantityScrap,
                MaterialsConsumed = order.MaterialRequirements.Select(r => new MaterialConsumed
                {
                    RawMaterialId = r.RawMaterialId,
                    MaterialCode = r.MaterialCode,
                    QuantityConsumed = r.QuantityConsumed,
                    QuantityReturned = r.QuantityReserved - r.QuantityConsumed,
                    Unit = r.Unit
                }).ToList()
            };
            await _eventPublisher.PublishAsync(completionEvent);

            _logger.LogInformation(
                "Completed ProductionOrder {OrderNumber}: Good={Good}, Scrap={Scrap}",
                order.OrderNumber, dto.QuantityGood, dto.QuantityScrap);
        }

        /// <summary>
        /// Cancel production order — Saga pattern for material return
        /// </summary>
        public async Task CancelAsync(Guid id, string reason)
        {
            var order = await _repository.GetByIdWithDetailsAsync(id);
            if (order == null)
                throw new NotFoundException("ProductionOrder", id);

            // Block cancel from Completed and InProgress
            if (order.Status == ProductionOrderStatus.Completed)
                throw new BusinessRuleException("Cannot cancel completed order");

            if (order.Status == ProductionOrderStatus.InProgress)
                throw new BusinessRuleException(
                    "Cannot cancel order that is in progress. Complete it first.");

            // Track if materials were reserved (for Saga)
            var wasReserved = order.ReservationStatus == ReservationStatus.Reserved;

            // Update status
            order.Status = ProductionOrderStatus.Cancelled;
            order.CancelReason = reason;
            order.CancelledAt = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;
            order.Notes = $"Cancelled: {reason}";

            await _repository.UpdateAsync(order);

            if (wasReserved)
            {
                // SAGA: Return reserved materials to Inventory
                var returnEvent = new MaterialReturnRequestedEvent
                {
                    ProductionOrderId = order.Id,
                    ProductionOrderNumber = order.OrderNumber,
                    MaterialsConsumed = order.MaterialRequirements.Select(r => new MaterialConsumed
                    {
                        RawMaterialId = r.RawMaterialId,
                        MaterialCode = r.MaterialCode,
                        QuantityConsumed = 0, // Nothing consumed yet
                        QuantityReturned = r.QuantityReserved, // Return all reserved
                        Unit = r.Unit
                    }).ToList()
                };
                await _eventPublisher.PublishAsync(returnEvent);

                _logger.LogInformation(
                    "Cancelled ProductionOrder {OrderNumber} — Saga: returning reserved materials",
                    order.OrderNumber);
            }
            else
            {
                // Simple cancel — no materials to return
                var cancelEvent = new ProductionOrderCancelledEvent
                {
                    ProductionOrderId = order.Id,
                    ProductionOrderNumber = order.OrderNumber,
                    Reason = reason
                };
                await _eventPublisher.PublishAsync(cancelEvent);

                _logger.LogInformation(
                    "Cancelled ProductionOrder {OrderNumber}: {Reason}",
                    order.OrderNumber, reason);
            }
        }

        // ========================================
        // HELPER METHODS
        // ========================================

        /// <summary>
        /// Publish MaterialReservationRequestedEvent — reused by Release and Retry
        /// </summary>
        private async Task PublishReservationEvent(ProductionOrder order, Models.BOM? bom)
        {
            var reservationEvent = new MaterialReservationRequestedEvent
            {
                ProductionOrderId = order.Id,
                ProductionOrderNumber = order.OrderNumber,
                BomCode = bom?.BomCode,
                BomVersion = bom?.Version,
                Materials = order.MaterialRequirements.Select(m => new MaterialToReserve
                {
                    RawMaterialId = m.RawMaterialId,
                    MaterialCode = m.MaterialCode,
                    Quantity = m.QuantityRequired,
                    Unit = m.Unit
                }).ToList()
            };
            await _eventPublisher.PublishAsync(reservationEvent);
        }

        private static ProductionOrderDto MapToDto(ProductionOrder order)
        {
            return new ProductionOrderDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                SalesOrderId = order.SalesOrderId,
                SalesOrderNumber = order.SalesOrderNumber,
                ProductId = order.ProductId,
                ProductCode = order.ProductCode,
                ProductName = order.ProductName,
                BOMId = order.BOMId,
                QuantityPlanned = order.QuantityPlanned,
                QuantityGood = order.QuantityGood,
                QuantityScrap = order.QuantityScrap,
                QuantityProduced = order.QuantityGood + order.QuantityScrap,
                PercentComplete = order.QuantityPlanned > 0 
                    ? Math.Round((order.QuantityGood + order.QuantityScrap) / order.QuantityPlanned * 100, 2)
                    : 0,
                PlannedStartDate = order.PlannedStartDate,
                PlannedEndDate = order.PlannedEndDate,
                ActualStartDate = order.ActualStartDate,
                ActualEndDate = order.ActualEndDate,
                Status = order.Status,
                Priority = order.Priority,
                Notes = order.Notes,
                CreatedAt = order.CreatedAt,
                // New fields
                ReservationStatus = order.ReservationStatus,
                ReservationFailReason = order.ReservationFailReason,
                ReservationAttempts = order.ReservationAttempts,
                BomCode = order.BomCode,
                BomVersion = order.BomVersion,
                CancelReason = order.CancelReason,
                CancelledAt = order.CancelledAt,
                MaterialsReturned = order.MaterialsReturned,
                ReleasedAt = order.ReleasedAt,
                ReleasedBy = order.ReleasedBy,
                MaterialRequirements = order.MaterialRequirements.Select(r => new MaterialRequirementDto
                {
                    Id = r.Id,
                    RawMaterialId = r.RawMaterialId,
                    MaterialCode = r.MaterialCode,
                    MaterialName = r.MaterialName,
                    QuantityRequired = r.QuantityRequired,
                    QuantityReserved = r.QuantityReserved,
                    QuantityConsumed = r.QuantityConsumed,
                    Unit = r.Unit,
                    Status = r.Status
                }).ToList()
            };
        }
    }
}
