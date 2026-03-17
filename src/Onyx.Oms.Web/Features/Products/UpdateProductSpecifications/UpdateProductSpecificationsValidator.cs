using FluentValidation;

namespace Onyx.Oms.Web.Features.Products.UpdateProductSpecifications
{
    public class UpdateProductSpecificationsValidator : AbstractValidator<UpdateProductSpecificationsCommand>
    {
        public UpdateProductSpecificationsValidator()
        {
            RuleFor(x => x.Specifications)
                .NotNull().WithMessage("Specifications dictionary cannot be null.");
        }
    }
}
