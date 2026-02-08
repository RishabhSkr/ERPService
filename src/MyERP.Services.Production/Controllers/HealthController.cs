/*
 * HealthController - Health Check Endpoint
 * 
 * 📚 INDUSTRY vs NOOB:
 * 
 * ❌ NOOB: No health endpoint, just test manually
 * ✅ INDUSTRY:
 *    1. /health endpoint for load balancers, Kubernetes
 *    2. Check dependencies (DB, Redis, RabbitMQ)
 *    3. Return structured health status
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyERP.Services.Production.Data;

namespace MyERP.Services.Production.Controllers
{
    [ApiController]
    [Route("api/production/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly ProductionDbContext _context;
        private readonly ILogger<HealthController> _logger;

        public HealthController(ProductionDbContext context, ILogger<HealthController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Simple health check - returns 200 if service is running
        /// Used by: Load balancers, Docker health checks, Kubernetes probes
        /// </summary>
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                status = "healthy",
                service = "MyERP.Services.Production",
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Detailed health check - checks database connectivity
        /// </summary>
        [HttpGet("detailed")]
        public async Task<IActionResult> GetDetailed()
        {
            var health = new
            {
                status = "healthy",
                service = "MyERP.Services.Production",
                timestamp = DateTime.UtcNow,
                checks = new Dictionary<string, object>()
            };

            // Check database
            try
            {
                await _context.Database.CanConnectAsync();
                health.checks["database"] = new { status = "healthy" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database health check failed");
                health.checks["database"] = new { status = "unhealthy", error = ex.Message };
            }

            return Ok(health);
        }
    }
}
