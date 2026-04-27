using Microsoft.AspNetCore.Mvc;
using MyERP.Services.Production.DTOs.ProcessRoute;
using MyERP.Services.Production.Middleware;
using MyERP.Services.Production.Services.ProcessRoute;
using Microsoft.AspNetCore.Authorization;

namespace MyERP.Services.Production.Controllers
{
    [ApiController]
    [Route("api/production/process-routes")]
    [Authorize]
    public class ProcessRoutesController : ControllerBase
    {
        private readonly IProcessRouteService _service;
        public ProcessRoutesController(IProcessRouteService service) => _service = service;

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProcessRouteDto>>>> GetAll() =>
            Ok(ApiResponse<IEnumerable<ProcessRouteDto>>.Ok(await _service.GetAllAsync()));

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ApiResponse<ProcessRouteDto>>> GetById(Guid id) =>
            Ok(ApiResponse<ProcessRouteDto>.Ok(await _service.GetByIdAsync(id)));

        [HttpGet("product/{productId:guid}")]
        public async Task<ActionResult<ApiResponse<ProcessRouteDto>>> GetByProduct(Guid productId)
        {
            var result = await _service.GetByProductIdAsync(productId);
            if (result == null)
                return NotFound(ApiResponse<ProcessRouteDto>.Fail("No active route for this product"));
            return Ok(ApiResponse<ProcessRouteDto>.Ok(result));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<ProcessRouteDto>>> Create([FromBody] CreateProcessRouteDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.ProcessRouteId },
                ApiResponse<ProcessRouteDto>.Ok(result, "ProcessRoute created successfully"));
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ApiResponse<ProcessRouteDto>>> Update(Guid id, [FromBody] CreateProcessRouteDto dto) =>
            Ok(ApiResponse<ProcessRouteDto>.Ok(await _service.UpdateAsync(id, dto), "ProcessRoute updated"));
    }
}
