using FluentValidation;
using MyERP.SalesServiceTutorial.DTOs.Customers;

namespace MyERP.SalesServiceTutorial.Validators;
public class CreateCustomerValidator : AbstractValidator<CreateCustomerDto>
{  
    public CreateCustomerValidator()
    {   // obj-> Dto ka object
        RuleFor(obj => obj.Name)
                .NotEmpty()
                .WithMessage("Name is required")
                .MaximumLength(100)
                .WithMessage("Name must be less than 100 characters");
        RuleFor(obj => obj.Email)
                .NotEmpty()
                .WithMessage("Email is required")
                .MaximumLength(100)
                .WithMessage("Email must be less than 100 characters");
        RuleFor(obj => obj.Phone)
                .NotEmpty()
                .WithMessage("Phone is required")
                .MaximumLength(13)
                .WithMessage("Phone must be less than 13 characters");
        RuleFor(obj => obj.Address)
                .NotEmpty()
                .WithMessage("Address is required")
                .MaximumLength(100)
                .WithMessage("Address must be less than 100 characters");
        RuleFor(obj => obj.City)
                .NotEmpty()
                .WithMessage("City is required")
                .MaximumLength(100)
                .WithMessage("City must be less than 100 characters");
        RuleFor(obj => obj.Country)
                .NotEmpty()
                .WithMessage("Country is required")
                .MaximumLength(100)
                .WithMessage("Country must be less than 100 characters");

    }
}

public class UpdateCustomerValidator : AbstractValidator<UpdateCustomerDto>
{
    public UpdateCustomerValidator()
    {
        RuleFor(obj => obj.Name)
                .NotEmpty()
                .WithMessage("Name is required")
                .MaximumLength(100)
                .WithMessage("Name must be less than 100 characters");
        RuleFor(obj => obj.Email)
                .NotEmpty()
                .WithMessage("Email is required")
                .MaximumLength(100)
                .WithMessage("Email must be less than 100 characters");
        RuleFor(obj => obj.Phone)
                .NotEmpty()
                .WithMessage("Phone is required")
                .MaximumLength(13)
                .WithMessage("Phone must be less than 13 characters");
        RuleFor(obj => obj.Address)
                .NotEmpty()
                .WithMessage("Address is required")
                .MaximumLength(100)
                .WithMessage("Address must be less than 100 characters");
        RuleFor(obj => obj.City)
                .NotEmpty()
                .WithMessage("City is required")
                .MaximumLength(100)
                .WithMessage("City must be less than 100 characters");
        RuleFor(obj => obj.Country)
                .NotEmpty()
                .WithMessage("Country is required")
                .MaximumLength(100)
                .WithMessage("Country must be less than 100 characters");
    }
}
