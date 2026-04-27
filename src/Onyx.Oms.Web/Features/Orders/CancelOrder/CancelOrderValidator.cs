using FluentValidation;

namespace Onyx.Oms.Web.Features.Orders.CancelOrder
{
    public class CancelOrderValidator : AbstractValidator<CancelOrderCommand>
    {
        public CancelOrderValidator()
        {
            RuleFor(x => x.OrderId).NotEmpty();
        }
    }
}
