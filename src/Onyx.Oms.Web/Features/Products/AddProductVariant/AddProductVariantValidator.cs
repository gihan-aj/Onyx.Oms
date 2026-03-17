using FluentValidation;

namespace Onyx.Oms.Web.Features.Products.AddProductVariant
{
    public class AddProductVariantValidator : AbstractValidator<AddProductVariantCommand>
    {
        public AddProductVariantValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("Product ID is required.");

            RuleFor(x => x.Attributes)
                .NotNull().NotEmpty().WithMessage("Attributes are required to create a variant.");

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
