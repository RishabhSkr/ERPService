using FluentValidation;
using MyERP.Services.Inventory.DTOs.Units;

namespace MyERP.Services.Inventory.Validators
{
    public class CreateUnitDtoValidator : AbstractValidator<CreateUnitDto>
    {
        public CreateUnitDtoValidator()
        {
            RuleFor(x => x.UnitName)
                .NotEmpty().WithMessage("Unit name is required")
                .MinimumLength(2).WithMessage("Unit name must be at least 2 characters")
                .MaximumLength(50).WithMessage("Unit name cannot exceed 50 characters");

            RuleFor(x => x.UnitCode)
                .NotEmpty().WithMessage("Unit code is required")
                .MinimumLength(1).WithMessage("Unit code must be at least 1 character")
                .MaximumLength(10).WithMessage("Unit code cannot exceed 10 characters")
                .Matches(@"^[A-Z0-9]+$").WithMessage("Unit code must contain only uppercase letters and numbers");
        }
    }
}
