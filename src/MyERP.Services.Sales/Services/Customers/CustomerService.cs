using MyERP.Services.Sales.DTOs;
using MyERP.Services.Sales.DTOs.Customers;
using MyERP.Services.Sales.Exceptions;
using MyERP.Services.Sales.Models;
using MyERP.Services.Sales.Repositories.Customers;

namespace MyERP.Services.Sales.Services.Customers
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;

        public CustomerService(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<CustomerResponseDto> CreateAsync(CreateCustomerDto dto)
        {
            var exists = await _customerRepository.CodeExistsAsync(dto.CustomerCode);
            if (exists)
                throw new ConflictException($"Customer with code '{dto.CustomerCode}' already exists");

            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                CustomerCode = dto.CustomerCode.ToUpper(),
                CustomerName = dto.CustomerName,
                Email = dto.Email,
                Phone = dto.Phone,
                Address = dto.Address,
                City = dto.City,
                Country = dto.Country,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _customerRepository.AddAsync(customer);
            return await GetByIdAsync(customer.Id);
        }

        public async Task<PagedResponse<CustomerListDto>> GetAllAsync(
            int pageNumber = 1, int pageSize = 10, string? searchKeyword = null)
        {
            var (customers, totalCount) = await _customerRepository.GetAllAsync(pageNumber, pageSize, searchKeyword);

            var data = customers.Select(c => new CustomerListDto
            {
                Id = c.Id,
                CustomerCode = c.CustomerCode,
                CustomerName = c.CustomerName,
                Email = c.Email,
                Phone = c.Phone,
                City = c.City,
                IsActive = c.IsActive
            }).ToList();

            return new PagedResponse<CustomerListDto>(data, pageNumber, pageSize, totalCount);
        }

        public async Task<CustomerResponseDto> GetByIdAsync(Guid id)
        {
            var customer = await _customerRepository.GetByIdWithOrdersAsync(id);
            if (customer == null)
                throw new NotFoundException("Customer", id);

            return MapToDto(customer);
        }

        public async Task<CustomerResponseDto> UpdateAsync(Guid id, UpdateCustomerDto dto)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null)
                throw new NotFoundException("Customer", id);

            if (!string.IsNullOrWhiteSpace(dto.CustomerName))
                customer.CustomerName = dto.CustomerName;

            if (dto.Email != null)
                customer.Email = dto.Email;

            if (dto.Phone != null)
                customer.Phone = dto.Phone;

            if (dto.Address != null)
                customer.Address = dto.Address;

            if (dto.City != null)
                customer.City = dto.City;

            if (dto.Country != null)
                customer.Country = dto.Country;

            customer.UpdatedAt = DateTime.UtcNow;
            await _customerRepository.UpdateAsync(customer);

            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null)
                throw new NotFoundException("Customer", id);

            // Soft delete
            customer.IsActive = false;
            customer.UpdatedAt = DateTime.UtcNow;
            await _customerRepository.UpdateAsync(customer);

            return true;
        }

        public async Task<bool> RestoreAsync(Guid id)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null)
                throw new NotFoundException("Customer", id);

            if (customer.IsActive)
                throw new BadRequestException("Customer is already active");

            customer.IsActive = true;
            customer.UpdatedAt = DateTime.UtcNow;
            await _customerRepository.UpdateAsync(customer);

            return true;
        }

        private CustomerResponseDto MapToDto(Customer customer)
        {
            return new CustomerResponseDto
            {
                Id = customer.Id,
                CustomerCode = customer.CustomerCode,
                CustomerName = customer.CustomerName,
                Email = customer.Email,
                Phone = customer.Phone,
                Address = customer.Address,
                City = customer.City,
                Country = customer.Country,
                IsActive = customer.IsActive,
                OrderCount = customer.SalesOrders?.Count ?? 0,
                CreatedAt = customer.CreatedAt
            };
        }
    }
}
