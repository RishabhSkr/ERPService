using MyERP.Services.Sales.DTOs;
using MyERP.Services.Sales.DTOs.Customers;

namespace MyERP.Services.Sales.Services.Customers
{
    public interface ICustomerService
    {
        Task<CustomerResponseDto> CreateAsync(CreateCustomerDto dto);
        Task<PagedResponse<CustomerListDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10, string? searchKeyword = null);
        Task<CustomerResponseDto> GetByIdAsync(Guid id);
        Task<CustomerResponseDto> UpdateAsync(Guid id, UpdateCustomerDto dto);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> RestoreAsync(Guid id);
    }
}
