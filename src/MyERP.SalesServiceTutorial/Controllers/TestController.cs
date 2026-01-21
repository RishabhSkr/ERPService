
using Microsoft.AspNetCore.Mvc;
using MyERP.SalesServiceTutorial.Clients;
using MyERP.SalesServiceTutorial.Common.Responses;
using MyERP.SalesServiceTutorial.Events.Publishers;
using MyERP.SalesServiceTutorial.Events;


namespace MyERP.SalesServiceTutorial.Controllers;
[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly IInventoryServiceClient _inventoryClient;
    private readonly IEventPublisher _eventPublisher;
    
    public TestController(IInventoryServiceClient inventoryClient, IEventPublisher eventPublisher)
    {
        _inventoryClient = inventoryClient;
        _eventPublisher = eventPublisher;
    }
    
    [HttpGet("product/{id}")]
    public async Task<ActionResult> GetProductFromInventory(Guid id)
    {
        var product = await _inventoryClient.GetProductByIdAsync(id);
        
        if (product == null)
            return NotFound(ApiResponse.Fail($"Product {id} not found in Inventory"));
            
        return Ok(ApiResponse<ProductDto>.Ok($"Product found!", product));
    }
    [HttpPost("publish-event")]
    public async Task<ActionResult> PublishTestEvent()
    {
        var testEvent = new SalesOrderCreatedEvent
        {
            OrderId = 123,
            CustomerId = 456,
            TotalAmount = 999.99m,
            OrderDate = DateTime.UtcNow
        };
        
        await _eventPublisher.PublishAsync(testEvent);

        return Ok(ApiResponse.Ok("Event published to RabbitMQ!"));
    }
}