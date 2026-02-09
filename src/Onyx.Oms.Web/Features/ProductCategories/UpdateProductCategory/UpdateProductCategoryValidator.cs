using FluentValidation;

namespace Onyx.Oms.Web.Features.ProductCategories.UpdateProductCategory;

public class UpdateProductCategoryValidator : AbstractValidator<UpdateProductCategoryCommand>
{
    public UpdateProductCategoryValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty();

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
