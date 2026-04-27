/*
 * ProductionOrdersController - Manufacturing orders
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyERP.Services.Production.DTOs.ProductionOrder;
using MyERP.Services.Production.Middleware;
using MyERP.Services.Production.Services.ProductionOrders;
using System.Security.Claims;

namespace MyERP.Services.Production.Controllers
{
    [ApiController]
    [Route("api/production/orders")]
    [Authorize]
    public class ProductionOrdersController : ControllerBase
    {
        private readonly IProductionOrderService _service;
        private readonly ILogger<ProductionOrdersController> _logger;

        public ProductionOrdersController(
            IProductionOrderService service,
            ILogger<ProductionOrdersController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Get all production orders
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProductionOrderDto>>>> GetAll()
        {
            var orders = await _service.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<ProductionOrderDto>>.Ok(orders));
        }

        /// <summary>
        /// Get orders filtered by status
        /// </summary>
        [HttpGet("status/{status}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProductionOrderDto>>>> GetByStatus(string status)
        {
            var orders = await _service.GetByStatusAsync(status);
            return Ok(ApiResponse<IEnumerable<ProductionOrderDto>>.Ok(orders));
        }

        /// <summary>
        /// Get single order by ID with details
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ApiResponse<ProductionOrderDto>>> GetById(Guid id)
        {
            var order = await _service.GetByIdAsync(id);
            return Ok(ApiResponse<ProductionOrderDto>.Ok(order));
        }

        /// <summary>
        /// Create a manual production order (not from sales)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<ProductionOrderDto>>> Create(
            [FromBody] CreateProductionOrderDto dto)
        {
            var userId = GetCurrentUserId();
            var order = await _service.CreateAsync(dto, userId);
            
            return CreatedAtAction(
                nameof(GetById),
                new { id = order.Id },
                ApiResponse<ProductionOrderDto>.Ok(order, "Production order created"));
        }

        /// <summary>
        /// Start production (moves from Released to InProgress)
        /// </summary>
        [HttpPatch("{id:guid}/start")]
        public async Task<ActionResult<ApiResponse<string>>> Start(Guid id)
        {
            await _service.StartAsync(id);
            return Ok(new ApiResponse { Success = true, Message = "Production started" });
        }

        /// <summary>
        /// Update production progress (for IoT/machine updates)
        /// </summary>
        [HttpPut("{id:guid}/progress")]
        public async Task<ActionResult<ApiResponse<string>>> UpdateProgress(
            Guid id,
            [FromBody] UpdateProgressDto dto)
        {
            await _service.UpdateProgressAsync(id, dto);
            return Ok(new ApiResponse { Success = true, Message = "Progress updated" });
        }

        /// <summary>
        /// Complete production batch with good/scrap quantities
        /// </summary>
        [HttpPost("{id:guid}/complete")]
        public async Task<ActionResult<ApiResponse<string>>> Complete(
            Guid id,
            [FromBody] CompleteBatchDto dto)
        {
            await _service.CompleteAsync(id, dto);
            return Ok(new ApiResponse
            {
                Success = true,
                Message = $"Production completed: Good={dto.QuantityGood}, Scrap={dto.QuantityScrap}"
            });
        }

        /// <summary>
        /// Force-complete PO — auto-sums quantities from WOs (no user input needed)
        /// </summary>
        [HttpPost("{id:guid}/force-complete")]
        public async Task<ActionResult<ApiResponse<string>>> ForceComplete(Guid id)
        {
            await _service.ForceCompleteAsync(id);
            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Production order force-completed (quantities summed from Work Orders)"
            });
        }

        /// <summary>
        /// Cancel a production order
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<ApiResponse<string>>> Cancel(
            Guid id,
            [FromQuery] string reason = "Cancelled by user")
        {
            await _service.CancelAsync(id, reason);
            return Ok(new ApiResponse { Success = true, Message = "Order cancelled" });
        }

        /// <summary>
        /// Release a production order for manufacturing
        /// Moves from 'Create' to 'Released' with ReservationStatus=Pending
        /// </summary>
        [HttpPatch("{id:guid}/release")]
        public async Task<ActionResult<ApiResponse<string>>> Release(Guid id)
        {
            var userId = GetCurrentUserId();
            await _service.ReleaseAsync(id, userId);
            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Production order released. Material reservation requested."
            });
        }

        /// <summary>
        /// Retry failed material reservation
        /// Only works when ReservationStatus = 'Failed'
        /// </summary>
        [HttpPost("{id:guid}/retry-reservation")]
        public async Task<ActionResult<ApiResponse<string>>> RetryReservation(Guid id)
        {
            await _service.RetryReservationAsync(id);
            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Reservation retry initiated."
            });
        }

        private Guid? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
}
