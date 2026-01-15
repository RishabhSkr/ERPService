using FluentValidation;
using MyERP.Services.Inventory.DTOs.Products;

namespace MyERP.Services.Inventory.Validators
{
    public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
    {
        public CreateProductDtoValidator()
        {
            RuleFor(x => x.ProductCode)
                .NotEmpty().WithMessage("Product code is required")
                .MinimumLength(3).WithMessage("Product code must be at least 3 characters")
                .MaximumLength(50).WithMessage("Product code cannot exceed 50 characters")
                .Matches(@"^[A-Z0-9\-]+$").WithMessage("Product code must contain only uppercase letters, numbers, and hyphens");

            RuleFor(x => x.ProductName)
                .NotEmpty().WithMessage("Product name is required")
                .MinimumLength(3).WithMessage("Product name must be at least 3 characters")
                .MaximumLength(200).WithMessage("Product name cannot exceed 200 characters");

            RuleFor(x => x.CategoryId)
                .NotEmpty().WithMessage("Category is required");

            RuleFor(x => x.UnitId)
                .NotEmpty().WithMessage("Unit is required");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0");

            RuleFor(x => x.MinStockLevel)
                .GreaterThanOrEqualTo(0).WithMessage("Minimum stock level cannot be negative");
        }
    }
}
