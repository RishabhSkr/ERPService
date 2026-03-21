using Microsoft.AspNetCore.Mvc;
using MyERP.Services.Production.DTOs.WorkCenter;
using MyERP.Services.Production.Middleware;
using MyERP.Services.Production.Services.WorkCenter;

namespace MyERP.Services.Production.Controllers
{
    [ApiController]
    [Route("api/production/work-centers")]
    public class WorkCentersController : ControllerBase
    {
        private readonly IWorkCenterService _service;
        public WorkCentersController(IWorkCenterService service) => _service = service;

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<WorkCenterDto>>>> GetAll() =>
            Ok(ApiResponse<IEnumerable<WorkCenterDto>>.Ok(await _service.GetAllAsync()));

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ApiResponse<WorkCenterDto>>> GetById(Guid id) =>
            Ok(ApiResponse<WorkCenterDto>.Ok(await _service.GetByIdAsync(id)));

        [HttpPost]
        public async Task<ActionResult<ApiResponse<WorkCenterDto>>> Create([FromBody] CreateWorkCenterDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.WorkCenterId },
                ApiResponse<WorkCenterDto>.Ok(result, "WorkCenter created successfully"));
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ApiResponse<WorkCenterDto>>> Update(Guid id, [FromBody] CreateWorkCenterDto dto) =>
            Ok(ApiResponse<WorkCenterDto>.Ok(await _service.UpdateAsync(id, dto), "WorkCenter updated"));

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> Deactivate(Guid id)
        {
            await _service.DeactivateAsync(id);
            return Ok(new ApiResponse { Success = true, Message = "WorkCenter deactivated" });
        }
    }
}
