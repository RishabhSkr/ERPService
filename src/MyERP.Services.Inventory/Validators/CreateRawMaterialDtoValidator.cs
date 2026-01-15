using FluentValidation;
using MyERP.Services.Inventory.DTOs.RawMaterials;

namespace MyERP.Services.Inventory.Validators
{
    public class CreateRawMaterialDtoValidator : AbstractValidator<CreateRawMaterialDto>
    {
        public CreateRawMaterialDtoValidator()
        {
            RuleFor(x => x.MaterialCode)
                .NotEmpty().WithMessage("Material code is required")
                .MinimumLength(3).WithMessage("Material code must be at least 3 characters")
                .MaximumLength(50).WithMessage("Material code cannot exceed 50 characters")
                .Matches(@"^[A-Z0-9\-]+$").WithMessage("Material code must contain only uppercase letters, numbers, and hyphens");

            RuleFor(x => x.MaterialName)
                .NotEmpty().WithMessage("Material name is required")
                .MinimumLength(3).WithMessage("Material name must be at least 3 characters")
                .MaximumLength(200).WithMessage("Material name cannot exceed 200 characters");

            RuleFor(x => x.CategoryId)
                .NotEmpty().WithMessage("Category is required");

            RuleFor(x => x.UnitId)
                .NotEmpty().WithMessage("Unit is required");

            RuleFor(x => x.Cost)
                .GreaterThanOrEqualTo(0).WithMessage("Cost cannot be negative");

            RuleFor(x => x.MinStockLevel)
                .GreaterThanOrEqualTo(0).WithMessage("Minimum stock level cannot be negative");
        }
    }
}
