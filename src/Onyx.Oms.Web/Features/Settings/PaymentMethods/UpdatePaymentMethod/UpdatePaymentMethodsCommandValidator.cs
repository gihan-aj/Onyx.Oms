using FluentValidation;

namespace Onyx.Oms.Web.Features.Settings.PaymentMethods.UpdatePaymentMethod
{
    public class UpdatePaymentMethodsCommandValidator : AbstractValidator<UpdatePaymentMethodCommand>
    {
        public UpdatePaymentMethodsCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Payment method ID is required.");

            RuleFor(x => x.DisplayName)
                .NotEmpty().WithMessage("Display name is required.")
                .Length(1, 50).WithMessage("Display name must be between 1 and 50 characters.");

            RuleFor(x => x.FeeRate)
                .InclusiveBetween(0m, 100m).WithMessage("Fee rate must be between 0 and 100.");
        }
    }
}
