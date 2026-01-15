using FluentValidation;
using MyERP.Services.Inventory.DTOs.Categories;

namespace MyERP.Services.Inventory.Validators
{
    public class CreateCategoryDtoValidator : AbstractValidator<CreateCategoryDto>
    {
        public CreateCategoryDtoValidator()
        {
            RuleFor(x => x.CategoryName)
                .NotEmpty().WithMessage("Category name is required")
                .MinimumLength(2).WithMessage("Category name must be at least 2 characters")
                .MaximumLength(100).WithMessage("Category name cannot exceed 100 characters");

            RuleFor(x => x.CategoryCode)
                .NotEmpty().WithMessage("Category code is required")
                .MinimumLength(2).WithMessage("Category code must be at least 2 characters")
                .MaximumLength(20).WithMessage("Category code cannot exceed 20 characters")
                .Matches(@"^[A-Z0-9\-]+$").WithMessage("Category code must contain only uppercase letters, numbers, and hyphens");
        }
    }
}
