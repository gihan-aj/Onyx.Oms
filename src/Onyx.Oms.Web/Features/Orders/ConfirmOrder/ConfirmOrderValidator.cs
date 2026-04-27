using FluentValidation;

namespace Onyx.Oms.Web.Features.Orders.ConfirmOrder
{
    public class ConfirmOrderValidator : AbstractValidator<ConfirmOrderCommand>
    {
        public ConfirmOrderValidator()
        {
            RuleFor(x => x.OrderId).NotEmpty();
        }
    }
}
