using Microsoft.AspNetCore.Mvc;
using MyERP.Services.Production.DTOs.Equipment;
using MyERP.Services.Production.DTOs.Process;
using MyERP.Services.Production.Middleware;
using MyERP.Services.Production.Services.Equipment;
using Microsoft.AspNetCore.Authorization;

namespace MyERP.Services.Production.Controllers
{
    [ApiController]
    [Route("api/production/equipment")]
    [Authorize]
    public class EquipmentController : ControllerBase
    {
        private readonly IEquipmentService _service;
        public EquipmentController(IEquipmentService service) => _service = service;

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<EquipmentDto>>>> GetAll() =>
            Ok(ApiResponse<IEnumerable<EquipmentDto>>.Ok(await _service.GetAllAsync()));

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ApiResponse<EquipmentDto>>> GetById(Guid id) =>
            Ok(ApiResponse<EquipmentDto>.Ok(await _service.GetByIdAsync(id)));

        [HttpGet("work-center/{workCenterId:guid}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<EquipmentDto>>>> GetByWorkCenter(Guid workCenterId) =>
            Ok(ApiResponse<IEnumerable<EquipmentDto>>.Ok(await _service.GetByWorkCenterAsync(workCenterId)));

        [HttpPost]
        public async Task<ActionResult<ApiResponse<EquipmentDto>>> Create([FromBody] CreateEquipmentDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.EquipmentId },
                ApiResponse<EquipmentDto>.Ok(result, "Equipment created successfully"));
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ApiResponse<EquipmentDto>>> Update(Guid id, [FromBody] CreateEquipmentDto dto) =>
            Ok(ApiResponse<EquipmentDto>.Ok(await _service.UpdateAsync(id, dto), "Equipment updated"));

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> Deactivate(Guid id)
        {
            await _service.DeactivateAsync(id);
            return Ok(new ApiResponse { Success = true, Message = "Equipment deactivated" });
        }

        /// <summary>
        /// Link processes that this equipment can perform
        /// </summary>
        [HttpPost("{id:guid}/processes")]
        public async Task<ActionResult> LinkProcesses(Guid id, [FromBody] LinkProcessesDto dto)
        {
            await _service.LinkProcessesAsync(id, dto);
            return Ok(new ApiResponse { Success = true, Message = "Processes linked to equipment" });
        }

        [HttpGet("{id:guid}/processes")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProcessDto>>>> GetLinkedProcesses(Guid id) =>
            Ok(ApiResponse<IEnumerable<ProcessDto>>.Ok(await _service.GetLinkedProcessesAsync(id)));
    }
}
