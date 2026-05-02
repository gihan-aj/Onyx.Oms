using FluentValidation;

namespace Onyx.Oms.Web.Features.Orders.AddOrderPayment
{
    public class AddOrderPaymentCommandValidator : AbstractValidator<AddOrderPaymentCommand>
    {
        public AddOrderPaymentCommandValidator()
        {
            RuleFor(x => x.Currency)
                .NotEmpty().WithMessage("Currency is required.")
                .Length(3).WithMessage("Currency must be a 3-letter ISO code.");
            RuleFor(x => x.Method)
                .IsInEnum().WithMessage("Invalid payment method.");
            RuleFor(x => x.PaymentDate)
                .LessThanOrEqualTo(DateTimeOffset.UtcNow).WithMessage("Payment date cannot be in the future.");
        }
    }
}
