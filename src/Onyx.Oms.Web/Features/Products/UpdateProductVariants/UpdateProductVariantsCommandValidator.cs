using FluentValidation;

namespace Onyx.Oms.Web.Features.Products.UpdateProductVariants
{
    public class UpdateProductVariantsCommandValidator : AbstractValidator<UpdateProductVariantsCommand>
    {
        public UpdateProductVariantsCommandValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("Product ID is required.");

            RuleFor(x => x.Variants)
                .Must(variants => 
                {
                    var providedSkus = variants.Select(v => v.Sku).Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
                    return providedSkus.Distinct().Count() == providedSkus.Count;
                })
                .WithMessage("Each provided variant SKU must be unique in the request.");

            RuleForEach(x => x.Variants).SetValidator(new UpdateProductVariantDtoValidator());
        }
    }

    public class UpdateProductVariantDtoValidator : AbstractValidator<UpdateProductVariantDto>
    {
        public UpdateProductVariantDtoValidator()
        {
            RuleFor(x => x.Id)
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
