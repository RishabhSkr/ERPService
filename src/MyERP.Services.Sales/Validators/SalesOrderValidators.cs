using FluentValidation;
using MyERP.Services.Sales.DTOs.SalesOrders;

namespace MyERP.Services.Sales.Validators
{
    public class CreateSalesOrderDtoValidator : AbstractValidator<CreateSalesOrderDto>
    {
        public CreateSalesOrderDtoValidator()
        {
            RuleFor(x => x.CustomerId)
                .NotEmpty().WithMessage("Customer ID is required");

            RuleFor(x => x.Items)
                .NotEmpty().WithMessage("At least one item is required")
                .Must(items => items.Count > 0).WithMessage("At least one item is required");

            RuleForEach(x => x.Items).SetValidator(new CreateSalesOrderItemDtoValidator());
        }
    }

    public class CreateSalesOrderItemDtoValidator : AbstractValidator<CreateSalesOrderItemDto>
    {
        public CreateSalesOrderItemDtoValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("Product ID is required");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0");
        }
    }

    public class UpdateOrderStatusDtoValidator : AbstractValidator<UpdateOrderStatusDto>
    {
        private static readonly string[] ValidStatuses = { "Confirmed", "InProduction", "Shipped", "Delivered", "Cancelled" };

        public UpdateOrderStatusDtoValidator()
        {
            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Status is required")
                .Must(s => ValidStatuses.Contains(s))
                .WithMessage($"Status must be one of: {string.Join(", ", ValidStatuses)}");
        }
    }
}
