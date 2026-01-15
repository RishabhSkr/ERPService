using FluentValidation;
using MyERP.Services.Inventory.DTOs.RawMaterials;

namespace MyERP.Services.Inventory.Validators
{
    public class ReserveRawMaterialsDtoValidator : AbstractValidator<ReserveRawMaterialsDto>
    {
        public ReserveRawMaterialsDtoValidator()
        {
            RuleFor(x => x.ProductionOrderId)
                .NotEmpty().WithMessage("Production order ID is required");

            RuleFor(x => x.Materials)
                .NotEmpty().WithMessage("At least one material is required");

            RuleForEach(x => x.Materials).ChildRules(material =>
            {
                material.RuleFor(x => x.RawMaterialId)
                    .NotEmpty().WithMessage("Raw material ID is required");

                material.RuleFor(x => x.Quantity)
                    .GreaterThan(0).WithMessage("Quantity must be greater than 0");
            });
        }
    }
}
