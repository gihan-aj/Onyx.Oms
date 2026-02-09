using FluentValidation;

namespace Onyx.Oms.Web.Features.ProductCategories.CreateProductCategory;

public class CreateProductCategoryValidator : AbstractValidator<CreateProductCategoryCommand>
{
    public CreateProductCategoryValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .MaximumLength(100);

        RuleFor(c => c.Description)
            .MaximumLength(500);

        RuleFor(c => c.IconUrl)
            .MaximumLength(255);

        RuleFor(c => c.Color)
            .MaximumLength(20);
    }
}
