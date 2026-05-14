using FluentValidation;

namespace Onyx.Oms.Web.Features.Orders.CreateOrder
{
    public class CreateOrderValidator : AbstractValidator<CreateOrderCommand>
    {
        public CreateOrderValidator()
        {
            RuleFor(x => x.CustomerId)
                .NotEmpty().WithMessage("Customer ID is required.");

            RuleFor(c => c.Notes)
                .MaximumLength(4000);

            RuleFor(c => c.DeliveryInstructions)
                .MaximumLength(500);

            RuleFor(x => x.Items)
                .NotEmpty().WithMessage("At least one order item is required.")
                .Must(i => i != null && i.Count > 0).WithMessage("Items cannot be empty.");

            RuleForEach(x => x.Items).SetValidator(new OrderItemDtoValidator());

            RuleFor(x => x.ShippingAddress!)
                .SetValidator(new ShippingAddressDtoValidator())
                .When(x => x.ShippingAddress != null);

            RuleFor(x => x.ShippingFee!)
                .SetValidator(new MoneyDtoValidator())
                .When(x => x.ShippingFee != null);

            RuleFor(x => x.TaxAmount!)
                .SetValidator(new MoneyDtoValidator())
                .When(x => x.TaxAmount != null);

            RuleFor(x => x.Discount!)
                .SetValidator(new OrderDiscountDtoValidator())
                .When(x => x.Discount != null);

            RuleFor(x => x.Payment!)
                .SetValidator(new InitialPaymentDtoValidator())
                .When(x => x.Payment != null);
        }
    }

    public class OrderItemDtoValidator : AbstractValidator<OrderItemDto>
    {
        public OrderItemDtoValidator()
        {
            RuleFor(x => x.ProductVariantId)
                .NotEmpty().WithMessage("Product Variant ID is required.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero.");

            RuleFor(x => x.Discount!)
                .SetValidator(new OrderDiscountDtoValidator())
                .When(x => x.Discount != null);
        }
    }

    public class ShippingAddressDtoValidator : AbstractValidator<ShippingAddressDto>
    {
        public ShippingAddressDtoValidator()
        {
            RuleFor(x => x.Street).MaximumLength(200);
            RuleFor(x => x.City).MaximumLength(100);
            RuleFor(x => x.District).MaximumLength(100);
            RuleFor(x => x.State).MaximumLength(100);
            RuleFor(x => x.PostalCode).MaximumLength(20);
            RuleFor(x => x.Country).MaximumLength(100);
        }
    }

    public class MoneyDtoValidator : AbstractValidator<MoneyDto>
    {
        public MoneyDtoValidator()
        {
            RuleFor(x => x.Amount)
                .GreaterThanOrEqualTo(0).WithMessage("Amount cannot be negative.");

            RuleFor(x => x.Currency)
                .NotEmpty().WithMessage("Currency is required.")
                .Length(3).WithMessage("Currency must be a 3-letter code (e.g., 'LKR').");
        }
    }

    public class OrderDiscountDtoValidator : AbstractValidator<OrderDiscountDto>
    {
        public OrderDiscountDtoValidator()
        {
            RuleFor(x => x.Value)
                .GreaterThanOrEqualTo(0).WithMessage("Discount value cannot be negative.");

            RuleFor(x => x.Reason)
                .MaximumLength(500);
        }
    }

    public class InitialPaymentDtoValidator : AbstractValidator<InitialPaymentDto>
    {
        public InitialPaymentDtoValidator()
        {
            RuleFor(x => x.Amount)
                .NotNull().WithMessage("Amount is required.")
                .SetValidator(new MoneyDtoValidator());

            RuleFor(x => x.Reference)
                .MaximumLength(200);

            RuleFor(x => x.PaymentDate)
                .NotEmpty().WithMessage("Payment date is required.");
        }
    }
}
