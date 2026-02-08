/*
 * PendingRequestsController - Sales Order approval workflow
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyERP.Services.Production.DTOs.PendingRequest;
using MyERP.Services.Production.Middleware;
using MyERP.Services.Production.Services.PendingRequests;
using System.Security.Claims;

namespace MyERP.Services.Production.Controllers
{
    [ApiController]
    [Route("api/production/pending-requests")]
    // [Authorize] // Uncomment when JWT is set up
    public class PendingRequestsController : ControllerBase
    {
        private readonly IPendingRequestService _service;
        private readonly ILogger<PendingRequestsController> _logger;

        public PendingRequestsController(
            IPendingRequestService service,
            ILogger<PendingRequestsController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Get all pending requests (inbox from Sales)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<PendingRequestDto>>>> GetAll()
        {
            var requests = await _service.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<PendingRequestDto>>.Ok(requests));
        }

        /// <summary>
        /// Get only pending (unprocessed) requests
        /// </summary>
        [HttpGet("pending")]
        public async Task<ActionResult<ApiResponse<IEnumerable<PendingRequestDto>>>> GetPending()
        {
            var requests = await _service.GetPendingAsync();
            return Ok(ApiResponse<IEnumerable<PendingRequestDto>>.Ok(requests));
        }

        /// <summary>
        /// Get single request by ID
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ApiResponse<PendingRequestDto>>> GetById(Guid id)
        {
            var request = await _service.GetByIdAsync(id);
            return Ok(ApiResponse<PendingRequestDto>.Ok(request));
        }

        /// <summary>
        /// Approve a pending request
        /// Creates ProductionOrder(s) and triggers material reservation
        /// </summary>
        [HttpPost("{id:guid}/approve")]
        public async Task<ActionResult<ApiResponse<string>>> Approve(
            Guid id,
            [FromBody] ApproveRequestDto dto)
        {
            var userId = GetCurrentUserId();
            await _service.ApproveAsync(id, dto, userId);
            
            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Request approved, production orders created"
            });
        }

        /// <summary>
        /// Cancel a pending request
        /// </summary>
        [HttpPost("{id:guid}/cancel")]
        public async Task<ActionResult<ApiResponse<string>>> Cancel(
            Guid id,
            [FromBody] CancelRequestDto dto)
        {
            var userId = GetCurrentUserId();
            await _service.CancelAsync(id, dto, userId);
            
            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Request cancelled"
            });
        }

        private Guid? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
}
