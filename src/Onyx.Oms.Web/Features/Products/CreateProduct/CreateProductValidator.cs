using FluentValidation;

namespace Onyx.Oms.Web.Features.Products.CreateProduct
{
    public class CreateProductValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required.")
                .MaximumLength(200).WithMessage("Product name must not exceed 200 characters.");

            RuleFor(x => x.CategoryId)
                .NotEmpty().WithMessage("Category ID is required.");

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");

            RuleFor(x => x.BaseCost)
                .NotNull()
                .SetValidator(new MoneyDtoValidator());

            RuleFor(x => x.BasePrice)
                .NotNull()
                .SetValidator(new MoneyDtoValidator());

            RuleFor(x => x.BaseWeight)
                .SetValidator(new WeightDtoValidator()!)
                .When(x => x.BaseWeight != null);

            RuleFor(x => x.BaseStockOnHand)
                .GreaterThanOrEqualTo(0).WithMessage("Stock on hand cannot be negative.")
                .When(x => x.BaseStockOnHand.HasValue);

            RuleFor(x => x.Options)
                .Must(options => options.Count <= 3)
                .WithMessage("A product can have a maximum of 3 options (e.g., Size, Color, Material).");

            RuleForEach(x => x.Options).SetValidator(new ProductOptionDtoValidator());

            RuleForEach(x => x.Variants).SetValidator(new CreateProductVariantDtoValidator());

            RuleForEach(x => x.Images).SetValidator(new CreateProductImageDtoValidator());
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

    public class WeightDtoValidator : AbstractValidator<WeightDto>
    {
        public WeightDtoValidator()
        {
            RuleFor(x => x.Value)
                .GreaterThanOrEqualTo(0).WithMessage("Weight cannot be negative.");

            RuleFor(x => x.Unit)
                .NotEmpty().WithMessage("Weight unit is required.");
        }
    }

    public class ProductOptionDtoValidator : AbstractValidator<ProductOptionDto>
    {
        public ProductOptionDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Option name (e.g., 'Color') is required.");

            RuleFor(x => x.Values)
                .NotEmpty().WithMessage("At least one value is required for an option.")
                .Must(v => v != null && v.Count > 0).WithMessage("Option values cannot be empty.");
        }
    }

    public class CreateProductVariantDtoValidator : AbstractValidator<CreateProductVariantDto>
    {
        public CreateProductVariantDtoValidator()
        {
            // SKU is optional (auto-generated), but if provided, it can't be just whitespace.
            RuleFor(x => x.Sku)
                .Must(sku => string.IsNullOrEmpty(sku) || !string.IsNullOrWhiteSpace(sku))
                .WithMessage("Variant SKU cannot be whitespace.");

            RuleFor(x => x.StockOnHand)
                .GreaterThanOrEqualTo(0).WithMessage("Variant stock cannot be negative.");

            // Validate optional overrides if they exist
            RuleFor(x => x.Cost!)
                .SetValidator(new MoneyDtoValidator())
                .When(x => x.Cost != null);

            RuleFor(x => x.Price!)
                .SetValidator(new MoneyDtoValidator())
                .When(x => x.Price != null);

            RuleFor(x => x.Weight!)
                .SetValidator(new WeightDtoValidator())
                .When(x => x.Weight != null);
        }
    }

    public class CreateProductImageDtoValidator : AbstractValidator<CreateProductImageDto>
    {
        public CreateProductImageDtoValidator()
        {
            RuleFor(x => x.Url)
                .NotEmpty().WithMessage("Image URL is required.")
                .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
                .WithMessage("Image URL must be a valid absolute URI.");

            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0);
        }
    }
}
