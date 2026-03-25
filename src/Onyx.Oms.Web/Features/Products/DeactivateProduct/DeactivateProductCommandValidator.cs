using FluentValidation;

namespace Onyx.Oms.Web.Features.Products.DeactivateProduct
{
    public class DeactivateProductCommandValidator : AbstractValidator<DeactivateProductCommand>
    {
        public DeactivateProductCommandValidator()
        {
            RuleFor(x => x.ProductId).NotEmpty().WithMessage("Product ID is required.");
        }
    }
}
