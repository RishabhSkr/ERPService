using FluentValidation;
using MyERP.Services.Sales.DTOs.Customers;

namespace MyERP.Services.Sales.Validators
{
    public class CreateCustomerDtoValidator : AbstractValidator<CreateCustomerDto>
    {
        public CreateCustomerDtoValidator()
        {
            RuleFor(x => x.CustomerCode)
                .NotEmpty().WithMessage("Customer code is required")
                .MaximumLength(50).WithMessage("Customer code cannot exceed 50 characters")
                .Matches("^[A-Za-z0-9-]+$").WithMessage("Customer code can only contain letters, numbers, and hyphens");

            RuleFor(x => x.CustomerName)
                .NotEmpty().WithMessage("Customer name is required")
                .MaximumLength(200).WithMessage("Customer name cannot exceed 200 characters");

            RuleFor(x => x.Email)
                .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
                .WithMessage("Invalid email format");

            RuleFor(x => x.Phone)
                .MaximumLength(50).When(x => !string.IsNullOrEmpty(x.Phone))
                .WithMessage("Phone cannot exceed 50 characters");
        }
    }

    public class UpdateCustomerDtoValidator : AbstractValidator<UpdateCustomerDto>
    {
        public UpdateCustomerDtoValidator()
        {
            RuleFor(x => x.CustomerName)
                .MaximumLength(200).When(x => !string.IsNullOrEmpty(x.CustomerName))
                .WithMessage("Customer name cannot exceed 200 characters");

            RuleFor(x => x.Email)
                .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
                .WithMessage("Invalid email format");
        }
    }
}
