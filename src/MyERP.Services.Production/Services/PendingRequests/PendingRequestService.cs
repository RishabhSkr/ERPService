/*
 * PendingRequest Service Implementation
 * 
 * 📝 KEY BUSINESS LOGIC: Approval Flow
 *    1. Get pending request with items
 *    2. For each item, find active BOM
 *    3. Create ProductionOrder with material requirements
 *    4. Publish MaterialReservationRequestedEvent
 */

using MyERP.Services.Production.Constants;
using MyERP.Services.Production.DTOs.PendingRequest;
using MyERP.Services.Production.Events;
using MyERP.Services.Production.Events.Publishers;
using MyERP.Shared.Events;
using MyERP.Services.Production.Exceptions;
using MyERP.Services.Production.Models;
using MyERP.Services.Production.Repositories.BOM;
using MyERP.Services.Production.Repositories.PendingRequests;
using MyERP.Services.Production.Repositories.ProductionOrders;

namespace MyERP.Services.Production.Services.PendingRequests
{
    public class PendingRequestService : IPendingRequestService
    {
        private readonly IPendingRequestRepository _pendingRepo;
        private readonly IBOMRepository _bomRepo;
        private readonly IProductionOrderRepository _orderRepo;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILogger<PendingRequestService> _logger;

        public PendingRequestService(
            IPendingRequestRepository pendingRepo,
            IBOMRepository bomRepo,
            IProductionOrderRepository orderRepo,
            IEventPublisher eventPublisher,
            ILogger<PendingRequestService> logger)
        {
            _pendingRepo = pendingRepo;
            _bomRepo = bomRepo;
            _orderRepo = orderRepo;
            _eventPublisher = eventPublisher;
            _logger = logger;
        }

        public async Task<PendingRequestDto> GetByIdAsync(Guid id)
        {
            var request = await _pendingRepo.GetByIdWithItemsAsync(id);
            if (request == null)
                throw new NotFoundException("PendingRequest", id);
            
            return MapToDto(request);
        }

        public async Task<IEnumerable<PendingRequestDto>> GetAllAsync()
        {
            var requests = await _pendingRepo.GetAllAsync();
            return requests.Select(MapToDto);
        }

        public async Task<IEnumerable<PendingRequestDto>> GetPendingAsync()
        {
            var requests = await _pendingRepo.GetByStatusAsync(PendingRequestStatus.Pending);
            return requests.Select(MapToDto);
        }

        public async Task ApproveAsync(Guid id, ApproveRequestDto dto, Guid? userId = null)
        {
            var request = await _pendingRepo.GetByIdWithItemsAsync(id);
            if (request == null)
                throw new NotFoundException("PendingRequest", id);

            if (request.Status != PendingRequestStatus.Pending)
                throw new BusinessRuleException($"Cannot approve request with status '{request.Status}'");

            _logger.LogInformation("Approving request {Id} with {ItemCount} items", id, request.Items.Count);

            // Create ProductionOrder for each item
            foreach (var item in request.Items)
            {
                // Find active BOM for this product
                var bom = await _bomRepo.GetActiveByProductIdAsync(item.ProductId);
                if (bom == null)
                {
                    throw new BusinessRuleException(
                        $"No active BOM found for product '{item.ProductCode}'. Cannot create production order.");
                }

                // Calculate material requirements (BOM explosion)
                var materialRequirements = bom.Lines.Select(line => new MaterialRequirement
                {
                    Id = Guid.NewGuid(),
                    RawMaterialId = line.RawMaterialId,
                    MaterialCode = line.MaterialCode,
                    MaterialName = line.MaterialName,
                    // Required = BOM quantity × Production quantity × (1 + scrap%)
                    QuantityRequired = line.Quantity * item.Quantity * (1 + line.ScrapPercentage / 100),
                    Unit = line.Unit,
                    Status = MaterialRequirementStatus.Pending
                }).ToList();

                // Create ProductionOrder
                var orderNumber = await _orderRepo.GetNextOrderNumberAsync();
                var order = new ProductionOrder
                {
                    Id = Guid.NewGuid(),
                    OrderNumber = orderNumber,
                    SalesOrderId = request.SalesOrderId,
                    SalesOrderNumber = request.SalesOrderNumber,
                    ProductId = item.ProductId,
                    ProductCode = item.ProductCode,
                    ProductName = item.ProductName,
                    BOMId = bom.BOMId,
                    BomCode = bom.BomCode,
                    BomVersion = bom.Version,
                    QuantityPlanned = item.Quantity,
                    PlannedStartDate = dto.PlannedStartDate,
                    PlannedEndDate = dto.PlannedStartDate.AddDays(7), // Default 7 days
                    Status = ProductionOrderStatus.Create,
                    Priority = dto.Priority,
                    Notes = dto.Notes,
                    CreatedBy = userId,
                    MaterialRequirements = materialRequirements
                };

                await _orderRepo.CreateAsync(order);

                _logger.LogInformation(
                    "Created ProductionOrder {OrderNumber} for {ProductCode} × {Qty}",
                    orderNumber, item.ProductCode, item.Quantity);

                // Publish event to reserve materials
                var reservationEvent = new MaterialReservationRequestedEvent
                {
                    ProductionOrderId = order.Id,
                    ProductionOrderNumber = order.OrderNumber,
                    Materials = materialRequirements.Select(r => new MaterialToReserve
                    {
                        RawMaterialId = r.RawMaterialId,
                        MaterialCode = r.MaterialCode,
                        Quantity = r.QuantityRequired,
                        Unit = r.Unit
                    }).ToList()
                };

                await _eventPublisher.PublishAsync(reservationEvent);
            }

            // Update request status
            request.Status = PendingRequestStatus.Approved;
            request.ProcessedAt = DateTime.UtcNow;
            request.ProcessedBy = userId;
            await _pendingRepo.UpdateAsync(request);

            _logger.LogInformation("Request {Id} approved, created {Count} production orders", id, request.Items.Count);
        }

        public async Task CancelAsync(Guid id, CancelRequestDto dto, Guid? userId = null)
        {
            var request = await _pendingRepo.GetByIdAsync(id);
            if (request == null)
                throw new NotFoundException("PendingRequest", id);

            if (request.Status != PendingRequestStatus.Pending)
                throw new BusinessRuleException($"Cannot cancel request with status '{request.Status}'");

            request.Status = PendingRequestStatus.Cancelled;
            request.CancellationReason = dto.Reason;
            request.ProcessedAt = DateTime.UtcNow;
            request.ProcessedBy = userId;
            await _pendingRepo.UpdateAsync(request);

            _logger.LogInformation("Request {Id} cancelled: {Reason}", id, dto.Reason);
        }

        private static PendingRequestDto MapToDto(PendingRequest request)
        {
            return new PendingRequestDto
            {
                Id = request.Id,
                SalesOrderId = request.SalesOrderId,
                SalesOrderNumber = request.SalesOrderNumber,
                CustomerId = request.CustomerId,
                CustomerName = request.CustomerName,
                OrderDate = request.OrderDate,
                Status = request.Status,
                ReceivedAt = request.ReceivedAt,
                ProcessedAt = request.ProcessedAt,
                Items = request.Items.Select(i => new PendingRequestItemDto
                {
                    ProductId = i.ProductId,
                    ProductCode = i.ProductCode,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            };
        }
    }
}
