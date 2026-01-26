using Microsoft.AspNetCore.Mvc;

namespace MyERP.SalesServiceTutorial.Controllers;

[ApiController]
[Route("api/sales/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok( new
        {
            Status= "OK",
            Service = "Sales Service Tutorial",
            Timestamp = DateTime.UtcNow
        });
    }
}