using MyERP.Services.Sales.DTOs;
using MyERP.Services.Sales.DTOs.SalesOrders;
using MyERP.Services.Sales.Events;
using MyERP.Services.Sales.Exceptions;
using MyERP.Services.Sales.Models;
using MyERP.Services.Sales.Repositories.Customers;
using MyERP.Services.Sales.Repositories.SalesOrders;
using MyERP.Services.Sales.Services.External;

namespace MyERP.Services.Sales.Services.SalesOrders
{
    public class SalesOrderService : ISalesOrderService
    {
        private readonly ISalesOrderRepository _orderRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IInventoryServiceClient _inventoryClient;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILogger<SalesOrderService> _logger;

        public SalesOrderService(
            ISalesOrderRepository orderRepository,
            ICustomerRepository customerRepository,
            IInventoryServiceClient inventoryClient,
            IEventPublisher eventPublisher,
            ILogger<SalesOrderService> logger)
        {
            _orderRepository = orderRepository;
            _customerRepository = customerRepository;
            _inventoryClient = inventoryClient;
            _eventPublisher = eventPublisher;
            _logger = logger;
        }

        public async Task<SalesOrderResponseDto> CreateAsync(CreateSalesOrderDto dto, Guid? createdBy = null)
        {
            // 1. Validate customer exists
            var customerExists = await _customerRepository.ExistsAsync(dto.CustomerId);
            if (!customerExists)
                throw new NotFoundException("Customer", dto.CustomerId);

            // 2. Validate and fetch products from Inventory Service
            var orderItems = new List<SalesOrderItem>();
            decimal totalAmount = 0;

            foreach (var item in dto.Items)
            {
                var product = await _inventoryClient.GetProductByIdAsync(item.ProductId);
                if (product == null)
                    throw new NotFoundException($"Product with ID '{item.ProductId}' not found in Inventory");

                if (!product.IsActive)
                    throw new BadRequestException($"Product '{product.ProductName}' is not active");

                var totalPrice = product.Price * item.Quantity;
                totalAmount += totalPrice;

                orderItems.Add(new SalesOrderItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
                    ProductCode = product.ProductCode,
                    ProductName = product.ProductName,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price,
                    TotalPrice = totalPrice,
                    CreatedAt = DateTime.UtcNow
                });
            }

            // 3. Generate order number and create order
            var orderNumber = await _orderRepository.GenerateOrderNumberAsync();
            var customer = await _customerRepository.GetByIdAsync(dto.CustomerId);

            var order = new SalesOrder
            {
                Id = Guid.NewGuid(),
                OrderNumber = orderNumber,
                CustomerId = dto.CustomerId,
                OrderDate = DateTime.UtcNow,
                OrderStatus = "Pending",
                TotalAmount = totalAmount,
                Notes = dto.Notes,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow,
                Items = orderItems
            };

            await _orderRepository.AddAsync(order);

            _logger.LogInformation("Sales order {OrderNumber} created for customer {CustomerName}",
                orderNumber, customer?.CustomerName);

            // 4. Publish SalesOrderCreated event to RabbitMQ
            var @event = new SalesOrderCreatedEvent
            {
                SalesOrderId = order.Id,
                OrderNumber = order.OrderNumber,
                CustomerId = order.CustomerId,
                CustomerName = customer?.CustomerName ?? "",
                OrderDate = order.OrderDate,
                Items = orderItems.Select(i => new SalesOrderItemEvent
                {
                    ProductId = i.ProductId,
                    ProductCode = i.ProductCode,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            };

            await _eventPublisher.PublishAsync(@event);

            return await GetByIdAsync(order.Id);
        }

        public async Task<PagedResponse<SalesOrderListDto>> GetAllAsync(
            int pageNumber = 1, int pageSize = 10, string? status = null, Guid? customerId = null)
        {
            var (orders, totalCount) = await _orderRepository.GetAllAsync(pageNumber, pageSize, status, customerId);

            var data = orders.Select(o => new SalesOrderListDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                CustomerName = o.Customer?.CustomerName ?? "",
                OrderDate = o.OrderDate,
                OrderStatus = o.OrderStatus,
                TotalAmount = o.TotalAmount,
                ItemCount = o.Items?.Count ?? 0
            }).ToList();

            return new PagedResponse<SalesOrderListDto>(data, pageNumber, pageSize, totalCount);
        }

        public async Task<SalesOrderResponseDto> GetByIdAsync(Guid id)
        {
            var order = await _orderRepository.GetByIdWithItemsAsync(id);
            if (order == null)
                throw new NotFoundException("SalesOrder", id);

            return MapToDto(order);
        }

        public async Task<SalesOrderResponseDto> UpdateStatusAsync(Guid id, UpdateOrderStatusDto dto)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
                throw new NotFoundException("SalesOrder", id);

            // Validate status transition
            ValidateStatusTransition(order.OrderStatus, dto.Status);

            order.OrderStatus = dto.Status;
            if (!string.IsNullOrWhiteSpace(dto.Notes))
                order.Notes = dto.Notes;
            order.UpdatedAt = DateTime.UtcNow;

            await _orderRepository.UpdateAsync(order);

            _logger.LogInformation("Sales order {OrderNumber} status updated to {Status}",
                order.OrderNumber, dto.Status);

            return await GetByIdAsync(id);
        }

        public async Task<bool> CancelAsync(Guid id, string? reason = null)
        {
            var order = await _orderRepository.GetByIdWithItemsAsync(id);
            if (order == null)
                throw new NotFoundException("SalesOrder", id);

            if (order.OrderStatus == "Cancelled")
                throw new BadRequestException("Order is already cancelled");

            if (order.OrderStatus == "Delivered")
                throw new BadRequestException("Cannot cancel a delivered order");

            order.OrderStatus = "Cancelled";
            order.Notes = string.IsNullOrWhiteSpace(reason) ? order.Notes : $"Cancelled: {reason}";
            order.UpdatedAt = DateTime.UtcNow;

            await _orderRepository.UpdateAsync(order);

            _logger.LogInformation("Sales order {OrderNumber} cancelled", order.OrderNumber);

            // Publish SalesOrderCancelled event
            var @event = new SalesOrderCancelledEvent
            {
                SalesOrderId = order.Id,
                OrderNumber = order.OrderNumber,
                Reason = reason ?? "Order cancelled by user"
            };

            await _eventPublisher.PublishAsync(@event);

            return true;
        }

        private static void ValidateStatusTransition(string currentStatus, string newStatus)
        {
            var validTransitions = new Dictionary<string, string[]>
            {
                { "Pending", new[] { "Confirmed", "Cancelled" } },
                { "Confirmed", new[] { "InProduction", "Cancelled" } },
                { "InProduction", new[] { "Shipped", "Cancelled" } },
                { "Shipped", new[] { "Delivered" } },
                { "Delivered", Array.Empty<string>() },
                { "Cancelled", Array.Empty<string>() }
            };

            if (!validTransitions.ContainsKey(currentStatus))
                throw new BadRequestException($"Unknown current status: {currentStatus}");

            if (!validTransitions[currentStatus].Contains(newStatus))
                throw new BadRequestException($"Cannot transition from '{currentStatus}' to '{newStatus}'");
        }

        private static SalesOrderResponseDto MapToDto(SalesOrder order)
        {
            return new SalesOrderResponseDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                Customer = order.Customer != null ? new CustomerInfo
                {
                    Id = order.Customer.Id,
                    CustomerCode = order.Customer.CustomerCode,
                    CustomerName = order.Customer.CustomerName
                } : null,
                OrderDate = order.OrderDate,
                OrderStatus = order.OrderStatus,
                TotalAmount = order.TotalAmount,
                Notes = order.Notes,
                Items = order.Items?.Select(i => new SalesOrderItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductCode = i.ProductCode,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    TotalPrice = i.TotalPrice
                }).ToList() ?? new List<SalesOrderItemDto>(),
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt
            };
        }
    }
}
