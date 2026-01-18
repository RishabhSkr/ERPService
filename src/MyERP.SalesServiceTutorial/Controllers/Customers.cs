using Microsoft.AspNetCore.Mvc;
using MyERP.SalesServiceTutorial.DTOs.Customers;
using MyERP.SalesServiceTutorial.Models;
using MyERP.SalesServiceTutorial.Services.Customers;
using MyERP.SalesServiceTutorial.Common.Responses;

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
    public async Task<ActionResult<IEnumerable<CustomerResponseDto>>> GetAllCustomersAsync()
    {
        var customers = await _customerService.GetAllCustomersAsync();
        // Model to DTO
        var response = customers.Select(c => new CustomerResponseDto(
            c.Id,
            c.Name,
            c.Email,
            c.Phone,
            c.City,
            c.IsActive
        ));
        return Ok(ApiResponse<IEnumerable<CustomerResponseDto>>.Ok("Customers retrieved successfully", response));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerResponseDto>> GetCustomerByIdAsync(int id)
    {       
        
        var customer = await _customerService.GetCustomerByIdAsync(id);
        // Model to DTO 
        var response = new CustomerResponseDto(
            customer.Id,
            customer.Name,
            customer.Email,
            customer.Phone,
            customer.City,
            customer.IsActive
        );
        return Ok(ApiResponse<CustomerResponseDto>.Ok("Customer retrieved successfully", response));
    }

    [HttpPost]
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
            createdCustomer.City,
            createdCustomer.IsActive
           );
            return Created($"/api/customers/{createdCustomer.Id}", response);
    }
    
    [HttpPut("{id}")]
    public async Task<ActionResult<CustomerResponseDto>> UpdateCustomerAsync(int id,UpdateCustomerDto dto)
    {   
        var customer = new Customer {
            Name = dto.Name,
            Email = dto.Email,
            Phone = dto.Phone,
            Address = dto.Address,
            City = dto.City,
            Country = dto.Country,
        };
        var updatedCustomer = await _customerService.UpdateCustomerAsync(id,customer);
        var response = new CustomerResponseDto(
            updatedCustomer.Id,
            updatedCustomer.Name,
            updatedCustomer.Email,
            updatedCustomer.Phone,
            updatedCustomer.City,
            updatedCustomer.IsActive
        );
        return Ok(ApiResponse<CustomerResponseDto>.Ok("Customer updated successfully", response));
    }
    
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteCustomerAsync(int id)
    {
        
            await _customerService.DeleteCustomerAsync(id);
            return Ok(ApiResponse<CustomerResponseDto>.Ok("Customer deleted successfully", null));
        
    }
}