using FluentValidation;

namespace Onyx.Oms.Web.Features.Products.ActivateProduct
{
    public class ActivateProductCommandValidator : AbstractValidator<ActivateProductCommand>
    {
        public ActivateProductCommandValidator()
        {
            RuleFor(x => x.ProductId).NotEmpty().WithMessage("Product ID is required.");
        }
    }
}
