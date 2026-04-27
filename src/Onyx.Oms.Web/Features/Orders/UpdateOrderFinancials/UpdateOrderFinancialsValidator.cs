using FluentValidation;

namespace Onyx.Oms.Web.Features.Orders.UpdateOrderFinancials
{
    public class UpdateOrderFinancialsValidator : AbstractValidator<UpdateOrderFinancialsCommand>
    {
        public UpdateOrderFinancialsValidator()
        {
            RuleFor(x => x.OrderId).NotEmpty();
            RuleFor(x => x.Items).NotEmpty().Must(i => i != null && i.Count > 0);
            RuleForEach(x => x.Items).SetValidator(new UpdateOrderItemDtoValidator());
            
            RuleFor(x => x.ShippingFee!)
                .SetValidator(new UpdateMoneyDtoValidator())
                .When(x => x.ShippingFee != null);

            RuleFor(x => x.TaxAmount!)
                .SetValidator(new UpdateMoneyDtoValidator())
                .When(x => x.TaxAmount != null);

            RuleFor(x => x.Discount!)
                .SetValidator(new UpdateOrderDiscountDtoValidator())
                .When(x => x.Discount != null);
        }
    }

    public class UpdateOrderItemDtoValidator : AbstractValidator<UpdateOrderItemDto>
    {
        public UpdateOrderItemDtoValidator()
        {
            RuleFor(x => x.ProductVariantId).NotEmpty();
            RuleFor(x => x.Quantity).GreaterThan(0);
            RuleFor(x => x.Discount!)
                .SetValidator(new UpdateOrderDiscountDtoValidator())
                .When(x => x.Discount != null);
        }
    }

    public class UpdateMoneyDtoValidator : AbstractValidator<UpdateMoneyDto>
    {
        public UpdateMoneyDtoValidator()
        {
            RuleFor(x => x.Amount).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Currency).NotEmpty().Length(3);
        }
    }

    public class UpdateOrderDiscountDtoValidator : AbstractValidator<UpdateOrderDiscountDto>
    {
        public UpdateOrderDiscountDtoValidator()
        {
            RuleFor(x => x.Value).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Reason).MaximumLength(500);
        }
    }
}
