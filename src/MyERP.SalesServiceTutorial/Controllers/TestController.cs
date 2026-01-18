
using Microsoft.AspNetCore.Mvc;
using MyERP.SalesServiceTutorial.Clients;
using MyERP.SalesServiceTutorial.Common.Responses;
namespace MyERP.SalesServiceTutorial.Controllers;
[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly IInventoryServiceClient _inventoryClient;
    
    public TestController(IInventoryServiceClient inventoryClient)
    {
        _inventoryClient = inventoryClient;
    }
    
    [HttpGet("product/{id}")]
    public async Task<ActionResult> GetProductFromInventory(Guid id)
    {
        var product = await _inventoryClient.GetProductByIdAsync(id);
        
        if (product == null)
            return NotFound(ApiResponse.Fail($"Product {id} not found in Inventory"));
            
        return Ok(ApiResponse<ProductDto>.Ok($"Product found!", product));
    }
}