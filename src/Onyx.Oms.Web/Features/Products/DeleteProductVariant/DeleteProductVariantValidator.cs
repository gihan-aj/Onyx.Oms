using FluentValidation;

namespace Onyx.Oms.Web.Features.Products.DeleteProductVariant
{
    public class DeleteProductVariantValidator : AbstractValidator<DeleteProductVariantCommand>
    {
        public DeleteProductVariantValidator()
        {
            RuleFor(x => x.ProductId).NotEmpty().WithMessage("Product ID is required.");
            RuleFor(x => x.VariantId).NotEmpty().WithMessage("Variant ID is required.");
        }
    }
}
