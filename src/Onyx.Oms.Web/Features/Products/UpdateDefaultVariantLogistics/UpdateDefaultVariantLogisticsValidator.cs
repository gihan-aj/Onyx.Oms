using FluentValidation;

namespace Onyx.Oms.Web.Features.Products.UpdateDefaultVariantLogistics
{
    public class UpdateDefaultVariantLogisticsValidator : AbstractValidator<UpdateDefaultVariantLogisticsCommand>
    {
        public UpdateDefaultVariantLogisticsValidator()
        {
            RuleFor(x => x.ProductId).NotEmpty().WithMessage("Product ID is required.");
            RuleFor(x => x.Sku).NotEmpty().WithMessage("SKU is required.");
            
            RuleFor(x => x.Price).NotNull().WithMessage("Price is required.");
            When(x => x.Price != null, () => RuleFor(x => x.Price.Amount).GreaterThanOrEqualTo(0));
            
            RuleFor(x => x.Cost).NotNull().WithMessage("Cost is required.");
            When(x => x.Cost != null, () => RuleFor(x => x.Cost.Amount).GreaterThanOrEqualTo(0));
            
            When(x => x.Weight != null, () => RuleFor(x => x.Weight!.Value).GreaterThanOrEqualTo(0));
        }
    }
}
