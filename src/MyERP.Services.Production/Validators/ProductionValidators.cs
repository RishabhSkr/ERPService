/*
 * FluentValidation Validators
 * 
 * 📚 INDUSTRY vs NOOB:
 * 
 * ❌ NOOB: Validate in controller with if statements
 * ✅ INDUSTRY:
 *    1. Separate validator classes (Single Responsibility)
 *    2. Auto-registered from assembly
 *    3. Returns all errors at once (not one at a time)
 *    4. Reusable validation rules
 */

using FluentValidation;
using MyERP.Services.Production.DTOs.BOM;
using MyERP.Services.Production.DTOs.PendingRequest;
using MyERP.Services.Production.DTOs.ProductionOrder;

namespace MyERP.Services.Production.Validators
{
    public class CreateBOMDtoValidator : AbstractValidator<CreateBOMDto>
    {
        public CreateBOMDtoValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("ProductId is required");

            RuleFor(x => x.BOMCode)
                .NotEmpty().WithMessage("BOMCode is required")
                .MaximumLength(50).WithMessage("BOMCode cannot exceed 50 characters")
                .Matches("^[A-Z0-9-]+$").WithMessage("BOMCode must be uppercase alphanumeric with dashes");

            RuleFor(x => x.ProductName)
                .NotEmpty().WithMessage("ProductName is required")
                .MaximumLength(200).WithMessage("ProductName cannot exceed 200 characters");

            RuleFor(x => x.Lines)
                .NotEmpty().WithMessage("BOM must have at least one line");

            RuleForEach(x => x.Lines).SetValidator(new CreateBOMLineDtoValidator());
        }
    }

    public class CreateBOMLineDtoValidator : AbstractValidator<CreateBOMLineDto>
    {
        public CreateBOMLineDtoValidator()
        {
            RuleFor(x => x.RawMaterialId)
                .NotEmpty().WithMessage("RawMaterialId is required");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0");

            RuleFor(x => x.Unit)
                .NotEmpty().WithMessage("Unit is required");

            RuleFor(x => x.ScrapPercentage)
                .InclusiveBetween(0, 100).WithMessage("ScrapPercentage must be between 0 and 100");
        }
    }

    public class ApproveRequestDtoValidator : AbstractValidator<ApproveRequestDto>
    {
        public ApproveRequestDtoValidator()
        {
            RuleFor(x => x.PlannedStartDate)
                                .GreaterThanOrEqualTo(DateTime.Today).WithMessage("PlannedStartDate must be today or in the future");

            RuleFor(x => x.Priority)
                .InclusiveBetween(1, 5).WithMessage("Priority must be between 1 and 5");
        }
    }

    public class CancelRequestDtoValidator : AbstractValidator<CancelRequestDto>
    {
        public CancelRequestDtoValidator()
        {
            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Cancellation reason is required")
                .MinimumLength(10).WithMessage("Reason must be at least 10 characters");
        }
    }

    public class CreateProductionOrderDtoValidator : AbstractValidator<CreateProductionOrderDto>
    {
        public CreateProductionOrderDtoValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("ProductId is required");

            RuleFor(x => x.BOMId)
                .NotEmpty().WithMessage("BOMId is required");

            RuleFor(x => x.QuantityPlanned)
                .GreaterThan(0).WithMessage("QuantityPlanned must be greater than 0");

            RuleFor(x => x.PlannedStartDate)
                                .GreaterThanOrEqualTo(DateTime.Today).WithMessage("PlannedStartDate must be today or in the future");

            RuleFor(x => x.PlannedEndDate)
                .GreaterThanOrEqualTo(x => x.PlannedStartDate).WithMessage("PlannedEndDate must be after PlannedStartDate");

            RuleFor(x => x.Priority)
                .InclusiveBetween(1, 5).WithMessage("Priority must be between 1 and 5");
        }
    }

    public class CompleteBatchDtoValidator : AbstractValidator<CompleteBatchDto>
    {
        public CompleteBatchDtoValidator()
        {
            RuleFor(x => x.QuantityGood)
                .GreaterThanOrEqualTo(0).WithMessage("QuantityGood cannot be negative");

            RuleFor(x => x.QuantityScrap)
                .GreaterThanOrEqualTo(0).WithMessage("QuantityScrap cannot be negative");

            RuleFor(x => x)
                .Must(x => x.QuantityGood > 0 || x.QuantityScrap > 0)
                .WithMessage("Either QuantityGood or QuantityScrap must be greater than 0");
        }
    }
}
