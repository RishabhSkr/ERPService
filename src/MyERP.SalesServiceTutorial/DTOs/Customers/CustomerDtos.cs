namespace MyERP.SalesServiceTutorial.DTOs.Customers;

// For creating a new customer
public record CreateCustomerDto(
    string Name,
    string Email,
    string? Phone,
    string? Address,
    string? City,
    string? Country
);
// For updating a customer
public record UpdateCustomerDto(
    string Name,
    string Email,
    string? Phone,
    string? Address,
    string? City,
    string? Country
);
// For API responses
public record CustomerResponseDto(
    int Id,
    string Name,
    string Email,
    string? Phone,
    string? City,
    bool IsActive
);