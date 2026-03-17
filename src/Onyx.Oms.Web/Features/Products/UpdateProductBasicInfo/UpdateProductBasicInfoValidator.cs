using FluentValidation;

namespace Onyx.Oms.Web.Features.Products.UpdateProductBasicInfo
{
    public class UpdateProductBasicInfoValidator : AbstractValidator<UpdateProductBasicInfoCommand>
    {
        public UpdateProductBasicInfoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required.")
                .MaximumLength(200).WithMessage("Product name must not exceed 200 characters.");

            RuleFor(x => x.CategoryId)
                .NotEmpty().WithMessage("Category ID is required.");

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");

            RuleFor(x => x.BaseSku)
                .Must(sku => string.IsNullOrEmpty(sku) || !string.IsNullOrWhiteSpace(sku))
                .WithMessage("Variant SKU cannot be whitespace.");
        }
    }
}
