using Microsoft.AspNetCore.Mvc;
using MyERP.Services.Production.DTOs.WorkOrder;
using MyERP.Services.Production.Middleware;
using MyERP.Services.Production.Services.WorkOrder;

namespace MyERP.Services.Production.Controllers
{
    [ApiController]
    [Route("api/production")]
    public class WorkOrdersController : ControllerBase
    {
        private readonly IWorkOrderService _service;
        private readonly ILogger<WorkOrdersController> _logger;

        public WorkOrdersController(IWorkOrderService service, ILogger<WorkOrdersController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Generate Work Orders from Production Order + ProcessRoute
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
