using FluentValidation;

namespace Onyx.Oms.Web.Features.Products.CreateProduct;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.BaseSku)
            .MaximumLength(100);

        RuleFor(x => x.BaseCostAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.BasePriceAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.BaseWeightValue).GreaterThanOrEqualTo(0);

        RuleFor(x => x.BaseCostCurrency).NotEmpty().MaximumLength(3);
        RuleFor(x => x.BasePriceCurrency).NotEmpty().MaximumLength(3);
        RuleFor(x => x.BaseWeightUnit).NotEmpty().MaximumLength(10);

        RuleForEach(x => x.Variants).SetValidator(new ProductVariantDtoValidator());
        RuleForEach(x => x.Images).SetValidator(new ProductImageDtoValidator());
    }
}

public class ProductVariantDtoValidator : AbstractValidator<ProductVariantDto>
{
    public ProductVariantDtoValidator()
    {
        RuleFor(x => x.Sku).MaximumLength(100);
        RuleFor(x => x.Color).MaximumLength(50);
        RuleFor(x => x.Size).MaximumLength(50);
        
        RuleFor(x => x.CostAmount).GreaterThanOrEqualTo(0).When(x => x.CostAmount.HasValue);
        RuleFor(x => x.PriceAmount).GreaterThanOrEqualTo(0).When(x => x.PriceAmount.HasValue);
        RuleFor(x => x.WeightValue).GreaterThanOrEqualTo(0).When(x => x.WeightValue.HasValue);
        
        RuleFor(x => x.StockOnHand).GreaterThanOrEqualTo(0);
    }
}

public class ProductImageDtoValidator : AbstractValidator<ProductImageDto>
{
    public ProductImageDtoValidator()
    {
        RuleFor(x => x.Url).NotEmpty().MaximumLength(2048);
        RuleFor(x => x.Color).MaximumLength(50);
    }
}
