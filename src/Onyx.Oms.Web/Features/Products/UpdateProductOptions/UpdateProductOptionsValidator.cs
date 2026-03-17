using FluentValidation;

namespace Onyx.Oms.Web.Features.Products.UpdateProductOptions
{
    public class UpdateProductOptionsValidator : AbstractValidator<UpdateProductOptionsCommand>
    {
        public UpdateProductOptionsValidator()
        {
            RuleFor(x => x.Options)
                .NotNull().WithMessage("Options cannot be null.")
                .Must(o => o == null || o.Count <= 3).WithMessage("A product can have a maximum of 3 options.");

            RuleForEach(x => x.Options).SetValidator(new UpdateProductOptionDtoValidator());
        }
    }

    public class UpdateProductOptionDtoValidator : AbstractValidator<UpdateProductOptionDto>
    {
        public UpdateProductOptionDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Option name is required.");
            
            RuleFor(x => x.Values)
                .NotNull().NotEmpty().WithMessage("Option values cannot be empty.");
            
            RuleForEach(x => x.Values)
                .NotEmpty().WithMessage("Option value cannot be empty.");
        }
    }
}
