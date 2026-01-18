namespace MyERP.Services.Sales.DTOs.Customers
{
    public class CreateCustomerDto
    {
        public string CustomerCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
    }

    public class UpdateCustomerDto
    {
        public string? CustomerName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
    }

    public class CustomerResponseDto
    {
        public Guid Id { get; set; }
        public string CustomerCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public bool IsActive { get; set; }
        public int OrderCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CustomerListDto
    {
        public Guid Id { get; set; }
        public string CustomerCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? City { get; set; }
        public bool IsActive { get; set; }
    }
}
