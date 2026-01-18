using Microsoft.AspNetCore.Mvc;
using MyERP.SalesServiceTutorial.DTOs.Customers;
using MyERP.SalesServiceTutorial.Models;
using MyERP.SalesServiceTutorial.Services.Customers;
namespace MyERP.SalesServiceTutorial.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;
    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }
    
    [HttpGet]
    [Route("api/customers")]
    public async Task<ActionResult<IEnumerable<CustomerResponseDto>>> GetAllCustomersAsync()
    {
        var customers = await _customerService.GetAllCustomersAsync();
        // Model to DTO
        var response = customers.Select(c => new CustomerResponseDto(
            c.Id,
            c.Name,
            c.Email,
            c.Phone,
            c.Address,
            c.City,
            c.Country,
            c.IsActive
        ));
        return Ok(response);
    }

    [HttpGet]
    [Route("api/customers/{id}")]
    public async Task<ActionResult<CustomerResponseDto>> GetCustomerByIdAsync(int id)
    {       
        
        var customer = await _customerService.GetCustomerByIdAsync(id);
        if(customer == null) return NotFound();
        // Model to DTO
        var response = new CustomerResponseDto(
            customer.Id,
            customer.Name,
            customer.Email,
            customer.Phone,
            customer.Address,
            customer.City,
            customer.Country,
            customer.IsActive
        );
        return Ok(response);
    }

    [HttpPost]
    [Route("api/customers")]
    public async Task<ActionResult<CustomerResponseDto>> CreateCustomerAsync(CreateCustomerDto dto)
    {
            
        // convert DTO to Modeul (Customer)
        var customer = new Customer {
            Name = dto.Name,
            Email = dto.Email,
            Phone = dto.Phone,
            Address = dto.Address,
            City = dto.City,
            Country = dto.Country,
        }; 
            var createdCustomer = await _customerService.CreateCustomerAsync(customer);

           // convert Model to DTO
           var response =  new CustomerResponseDto (
            createdCustomer.Id,
            createdCustomer.Name,
            createdCustomer.Email,
            createdCustomer.Phone,
            createdCustomer.Address,
            createdCustomer.City,
            createdCustomer.Country,
           );
            return CreatedAtAction(nameof(GetCustomerByIdAsync),new {id = createdCustomer.Id},response); 
    }
    
    [HttpPut]
    [Route("api/customers/{id}")]
    public async Task<ActionResult<CustomerResponseDto>> UpdateCustomerAsync(int id,UpdateCustomerDto dto)
    {
        
        var customer = await _customerService.UpdateCustomerAsync(id,dto);
        return Ok(customer);
    }
    
    [HttpDelete]
    [Route("api/customers/{id}")]
    public async Task<ActionResult> DeleteCustomerAsync(int id)
    {
        
            await _customerService.DeleteCustomerAsync(id);
            return NoContent();
        
    }
}