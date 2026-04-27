using Microsoft.AspNetCore.Mvc;
using MyERP.Services.Production.DTOs.WorkOrder;
using MyERP.Services.Production.Middleware;
using MyERP.Services.Production.Services.WorkOrder;
using Microsoft.AspNetCore.Authorization;

namespace MyERP.Services.Production.Controllers
{
    [ApiController]
    [Route("api/production")]
    [Authorize]
    public class WorkOrdersController : ControllerBase
    {
        private readonly IWorkOrderService _service;
        private readonly ILogger<WorkOrdersController> _logger;

        public WorkOrdersController(IWorkOrderService service, ILogger<WorkOrdersController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // ====================================
        // NEW: Dashboard + Planning Info
        // ====================================

        /// <summary>
        /// Dashboard — POs with per-step WO aggregation
        /// </summary>
        [HttpGet("work-orders/dashboard")]
        public async Task<ActionResult<ApiResponse<IEnumerable<WorkOrderDashboardDto>>>> GetDashboard()
        {
            var result = await _service.GetDashboardAsync();
            return Ok(ApiResponse<IEnumerable<WorkOrderDashboardDto>>.Ok(result));
        }

        /// <summary>
        /// Planning Info — remaining qty + equipment per step before creating WO
        /// </summary>
        [HttpGet("work-orders/planning-info/{productionOrderId:guid}")]
        public async Task<ActionResult<ApiResponse<WorkOrderPlanningInfoDto>>> GetPlanningInfo(Guid productionOrderId)
        {
            var result = await _service.GetPlanningInfoAsync(productionOrderId);
            return Ok(ApiResponse<WorkOrderPlanningInfoDto>.Ok(result));
        }

        // ====================================
        // NEW: Create WO (User-specified qty)
        // ====================================

        /// <summary>
        /// Create Work Order — user specifies qty, step, workCenter
        /// </summary>
        [HttpPost("work-orders/create")]
        public async Task<ActionResult<ApiResponse<WorkOrderDto>>> Create([FromBody] CreateWorkOrderDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return StatusCode(201, ApiResponse<WorkOrderDto>.Ok(result, "Work Order created successfully"));
        }

        /// <summary>
        /// Release WO — reserve materials (BOM × WO qty)
        /// </summary>
        [HttpPatch("work-orders/{workOrderId:guid}/release")]
        public async Task<ActionResult<ApiResponse<WorkOrderDto>>> Release(Guid workOrderId)
        {
            var result = await _service.ReleaseAsync(workOrderId);
            return Ok(ApiResponse<WorkOrderDto>.Ok(result, "Work Order released, reservation pending"));
        }

        /// <summary>
        /// Cancel WO — with reason (any state except Completed)
        /// </summary>
        [HttpDelete("work-orders/{workOrderId:guid}")]
        public async Task<ActionResult<ApiResponse<string>>> Cancel(Guid workOrderId, [FromQuery] string reason)
        {
            await _service.CancelAsync(workOrderId, reason);
            return Ok(ApiResponse<string>.Ok("Work Order cancelled", $"Cancelled: {reason}"));
        }

        /// <summary>
        /// Retry failed reservation
        /// </summary>
        [HttpPost("work-orders/{workOrderId:guid}/retry-reservation")]
        public async Task<ActionResult<ApiResponse<string>>> RetryReservation(Guid workOrderId)
        {
            await _service.RetryReservationAsync(workOrderId);
            return Ok(ApiResponse<string>.Ok("Reservation retry initiated"));
        }

        // ====================================
        // EXISTING: Auto-generate (convenience)
        // ====================================

        /// <summary>
        /// Generate Work Orders from Production Order + ProcessRoute (auto — full PO qty)
        /// </summary>
        [HttpPost("orders/{productionOrderId:guid}/generate-work-orders")]
        public async Task<ActionResult<ApiResponse<IEnumerable<WorkOrderDto>>>> GenerateWorkOrders(Guid productionOrderId)
        {
            var result = await _service.GenerateWorkOrdersAsync(productionOrderId);
            return Ok(ApiResponse<IEnumerable<WorkOrderDto>>.Ok(result, $"Generated {result.Count()} work orders"));
        }

        /// <summary>
        /// Get all Work Orders for a Production Order
        /// </summary>
        [HttpGet("orders/{productionOrderId:guid}/work-orders")]
        public async Task<ActionResult<ApiResponse<IEnumerable<WorkOrderDto>>>> GetByPO(Guid productionOrderId) =>
            Ok(ApiResponse<IEnumerable<WorkOrderDto>>.Ok(await _service.GetByProductionOrderAsync(productionOrderId)));

        // ====================================
        // EXISTING: Equipment activation + tracking
        // ====================================

        /// <summary>
        /// Activate a Work Order on Equipment
        /// </summary>
        [HttpPost("work-orders/{workOrderId:guid}/activate")]
        public async Task<ActionResult<ApiResponse<WorkOrderExecutionDto>>> Activate(Guid workOrderId, [FromBody] ActivateWorkOrderDto dto)
        {
            var result = await _service.ActivateAsync(workOrderId, dto);
            return Ok(ApiResponse<WorkOrderExecutionDto>.Ok(result, "Work order activated on equipment"));
        }

        /// <summary>
        /// Complete execution on equipment
        /// </summary>
        [HttpPatch("work-orders/{workOrderId:guid}/executions/{executionId:guid}/complete")]
        public async Task<ActionResult<ApiResponse<WorkOrderExecutionDto>>> CompleteExecution(
            Guid workOrderId, Guid executionId, [FromBody] CompleteExecutionDto dto)
        {
            var result = await _service.CompleteExecutionAsync(workOrderId, executionId, dto);
            return Ok(ApiResponse<WorkOrderExecutionDto>.Ok(result, "Execution completed"));
        }

        /// <summary>
        /// Pause execution on equipment
        /// </summary>
        [HttpPatch("work-orders/{workOrderId:guid}/executions/{executionId:guid}/pause")]
        public async Task<ActionResult<ApiResponse<WorkOrderExecutionDto>>> PauseExecution(Guid workOrderId, Guid executionId)
        {
            var result = await _service.PauseExecutionAsync(workOrderId, executionId);
            return Ok(ApiResponse<WorkOrderExecutionDto>.Ok(result, "Execution paused"));
        }

        /// <summary>
        /// Get execution history for a Work Order
        /// </summary>
        [HttpGet("work-orders/{workOrderId:guid}/executions")]
        public async Task<ActionResult<ApiResponse<IEnumerable<WorkOrderExecutionDto>>>> GetExecutions(Guid workOrderId) =>
            Ok(ApiResponse<IEnumerable<WorkOrderExecutionDto>>.Ok(await _service.GetExecutionsAsync(workOrderId)));

        /// <summary>
        /// Get all executions on an equipment (tracking)
        /// </summary>
        [HttpGet("equipment/{equipmentId:guid}/executions")]
        public async Task<ActionResult<ApiResponse<IEnumerable<WorkOrderExecutionDto>>>> GetEquipmentExecutions(Guid equipmentId) =>
            Ok(ApiResponse<IEnumerable<WorkOrderExecutionDto>>.Ok(await _service.GetEquipmentExecutionsAsync(equipmentId)));
    }
}
