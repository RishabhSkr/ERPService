using Microsoft.AspNetCore.Mvc;
using MyERP.Services.Sales.DTOs;
using MyERP.Services.Sales.DTOs.Customers;
using MyERP.Services.Sales.Services.Customers;
using Microsoft.AspNetCore.Authorization;

namespace MyERP.Services.Sales.Controllers
{
    [ApiController]
    [Route("api/sales/customers")]
    [Authorize(Policy = "DynamicPermission")]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomersController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCustomerDto dto)
        {
            var result = await _customerService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id },
                ApiResponse<CustomerResponseDto>.Ok(result, "Customer created successfully"));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchKeyword = null)
        {
            var result = await _customerService.GetAllAsync(pageNumber, pageSize, searchKeyword);
            return Ok(ApiResponse<PagedResponse<CustomerListDto>>.Ok(result));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _customerService.GetByIdAsync(id);
            return Ok(ApiResponse<CustomerResponseDto>.Ok(result));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerDto dto)
        {
            var result = await _customerService.UpdateAsync(id, dto);
            return Ok(ApiResponse<CustomerResponseDto>.Ok(result, "Customer updated successfully"));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _customerService.DeleteAsync(id);
            return Ok(ApiResponse.Ok("Customer deleted successfully"));
        }

        [HttpPatch("{id}/restore")]
        public async Task<IActionResult> Restore(Guid id)
        {
            await _customerService.RestoreAsync(id);
            return Ok(ApiResponse.Ok("Customer restored successfully"));
        }
    }
}
