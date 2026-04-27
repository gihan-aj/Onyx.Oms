using FluentValidation;

namespace Onyx.Oms.Web.Features.Orders.DeliverOrder
{
    public class DeliverOrderValidator : AbstractValidator<DeliverOrderCommand>
    {
        public DeliverOrderValidator()
        {
            RuleFor(x => x.OrderId).NotEmpty();
        }
    }
}
