using Microsoft.AspNetCore.Mvc;
using MyERP.Services.Production.DTOs.MRP;
using MyERP.Services.Production.Services.MRP;
using Microsoft.AspNetCore.Authorization;

namespace MyERP.Services.Production.Controllers
{
    [ApiController]
    [Route("api/production/mrp")]
    [Authorize]
    public class MRPController : ControllerBase
    {
        private readonly IMRPService _mrpService;
        private readonly ILogger<MRPController> _logger;

        public MRPController(IMRPService mrpService, ILogger<MRPController> logger)
        {
            _mrpService = mrpService;
            _logger = logger;
        }

        /// <summary>
        /// Run MRP — calculate material needs and purchase suggestions
        /// </summary>
        [HttpPost("run")]
        public async Task<IActionResult> RunMRP([FromBody] RunMRPRequestDto? request)
        {
            var mrpRequest = request ?? new RunMRPRequestDto();

            _logger.LogInformation("MRP run requested with statuses: {Statuses}",
                string.Join(", ", mrpRequest.IncludeStatuses ?? new List<string> { "Create", "Released" }));

            var result = await _mrpService.RunAsync(mrpRequest);

            return Ok(new
            {
                success = true,
                message = $"MRP run complete: {result.TotalOrders} orders analyzed, {result.PurchaseSuggestions.Count} purchase suggestions",
                data = result
            });
        }
    }
}
