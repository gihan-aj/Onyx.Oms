using FluentValidation;

namespace Onyx.Oms.Web.Features.Products.UpdateProductVariantLogistics
{
    public class UpdateProductVariantLogisticsValidator : AbstractValidator<UpdateProductVariantLogisticsCommand>
    {
        public UpdateProductVariantLogisticsValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("Product ID is required.");

            RuleFor(x => x.VariantId)
                .NotEmpty().WithMessage("Variant ID is required.");

            When(x => x.Price != null, () =>
            {
                RuleFor(x => x.Price!.Amount)
                    .GreaterThanOrEqualTo(0).WithMessage("Price cannot be negative.");
            });

            When(x => x.Cost != null, () =>
            {
                RuleFor(x => x.Cost!.Amount)
                    .GreaterThanOrEqualTo(0).WithMessage("Cost cannot be negative.");
            });

            When(x => x.Weight != null, () =>
            {
                RuleFor(x => x.Weight!.Value)
                    .GreaterThanOrEqualTo(0).WithMessage("Weight cannot be negative.");
            });
        }
    }
}
