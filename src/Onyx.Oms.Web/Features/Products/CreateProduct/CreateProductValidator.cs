using FluentValidation;

namespace Onyx.Oms.Web.Features.Products.CreateProduct;

public class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(200);

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category is required.");

        RuleFor(x => x.BasePrice)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.BaseCost)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Variants)
            .NotEmpty().WithMessage("At least one variant must be created.");

        RuleForEach(x => x.Variants).ChildRules(variant =>
        {
            variant.RuleFor(v => v.Sku).NotEmpty().MaximumLength(50);
            variant.RuleFor(v => v.Name).NotEmpty().MaximumLength(200);
            variant.RuleFor(v => v.Size).NotEmpty().MaximumLength(50);
            variant.RuleFor(v => v.Color).NotEmpty().MaximumLength(50);
            variant.RuleFor(v => v.Price).GreaterThanOrEqualTo(0);
            variant.RuleFor(v => v.Cost).GreaterThanOrEqualTo(0);
        });

        RuleForEach(x => x.Images).ChildRules(image =>
        {
            image.RuleFor(i => i.Url).NotEmpty().WithMessage("Image URL/Path is required.");
        });
    }
}
