using Microsoft.AspNetCore.Mvc;
using MyERP.Services.Sales.DTOs;
using MyERP.Services.Sales.DTOs.SalesOrders;
using MyERP.Services.Sales.Services.SalesOrders;
using Microsoft.AspNetCore.Authorization;

namespace MyERP.Services.Sales.Controllers
{
    [ApiController]
    [Route("api/sales/orders")]
    // [Authorize(Policy = "DynamicPermission")]
    public class SalesOrdersController : ControllerBase
    {
        private readonly ISalesOrderService _orderService;

        public SalesOrdersController(ISalesOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSalesOrderDto dto)
        {
            // Get userId from JWT claims (safely handles non-GUID values like "99999999")
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            Guid? createdBy = userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId) 
                ? userId 
                : null;
            
            var result = await _orderService.CreateAsync(dto, createdBy);
            return CreatedAtAction(nameof(GetById), new { id = result.Id },
                ApiResponse<SalesOrderResponseDto>.Ok(result, "Sales order created successfully"));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? status = null,
            [FromQuery] Guid? customerId = null)
        {
            var result = await _orderService.GetAllAsync(pageNumber, pageSize, status, customerId);
            return Ok(ApiResponse<PagedResponse<SalesOrderListDto>>.Ok(result));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _orderService.GetByIdAsync(id);
            return Ok(ApiResponse<SalesOrderResponseDto>.Ok(result));
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusDto dto)
        {
            var result = await _orderService.UpdateStatusAsync(id, dto);
            return Ok(ApiResponse<SalesOrderResponseDto>.Ok(result, $"Order status updated to {dto.Status}"));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Cancel(Guid id, [FromQuery] string? reason = null)
        {
            await _orderService.CancelAsync(id, reason);
            return Ok(ApiResponse.Ok("Order cancelled successfully"));
        }
        // ====================================================================
        // DISPATCH — Send goods to customer
        // ====================================================================
        [HttpPost("{id}/dispatch")]
        public async Task<IActionResult> Dispatch(Guid id, [FromBody] DispatchRequestDto dto)
        {
            await _orderService.DispatchAsync(id, dto);
            return Ok(ApiResponse.Ok("Items dispatched successfully"));
        }

        // ====================================================================
        // MARK DELIVERED
        // ====================================================================
        [HttpPatch("{id}/deliver")]
        public async Task<IActionResult> MarkDelivered(Guid id)
        {
            await _orderService.MarkDeliveredAsync(id);
            return Ok(ApiResponse.Ok("Order marked as delivered"));
        }

        // ====================================================================
        // FULFILLMENT DASHBOARD
        // ====================================================================
        [HttpGet("fulfillment-dashboard")]
        public async Task<IActionResult> GetFulfillmentDashboard()
        {
            var result = await _orderService.GetFulfillmentDashboardAsync();
            return Ok(ApiResponse<List<FulfillmentDashboardDto>>.Ok(result));
        }

    }
}
