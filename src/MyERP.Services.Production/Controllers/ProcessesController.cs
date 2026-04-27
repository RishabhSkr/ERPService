using Microsoft.AspNetCore.Mvc;
using MyERP.Services.Production.DTOs.Process;
using MyERP.Services.Production.Middleware;
using MyERP.Services.Production.Services.Process;
using Microsoft.AspNetCore.Authorization;

namespace MyERP.Services.Production.Controllers
{
    [ApiController]
    [Route("api/production/processes")]
    [Authorize]
    public class ProcessesController : ControllerBase
    {
        private readonly IProcessService _service;
        private readonly ILogger<ProcessesController> _logger;

        public ProcessesController(IProcessService service, ILogger<ProcessesController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProcessDto>>>> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<ProcessDto>>.Ok(result));
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ApiResponse<ProcessDto>>> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            return Ok(ApiResponse<ProcessDto>.Ok(result));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<ProcessDto>>> Create([FromBody] CreateProcessDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.ProcessId },
                ApiResponse<ProcessDto>.Ok(result, "Process created successfully"));
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ApiResponse<ProcessDto>>> Update(Guid id, [FromBody] CreateProcessDto dto)
        {
            var result = await _service.UpdateAsync(id, dto);
            return Ok(ApiResponse<ProcessDto>.Ok(result, "Process updated successfully"));
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> Deactivate(Guid id)
        {
            await _service.DeactivateAsync(id);
            return Ok(new ApiResponse { Success = true, Message = "Process deactivated successfully" });
        }
    }
}
