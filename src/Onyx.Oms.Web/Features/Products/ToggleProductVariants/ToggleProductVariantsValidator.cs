using FluentValidation;

namespace Onyx.Oms.Web.Features.Products.ToggleProductVariants
{
    public class ToggleProductVariantsValidator : AbstractValidator<ToggleProductVariantsCommand>
    {
        public ToggleProductVariantsValidator()
        {
            // Simple validator for toggle check
            RuleFor(x => x.HasVariants)
                .NotNull().WithMessage("HasVariants boolean flag must be provided.");
        }
    }
}
