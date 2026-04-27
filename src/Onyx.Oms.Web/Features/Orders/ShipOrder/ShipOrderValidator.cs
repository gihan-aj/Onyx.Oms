using FluentValidation;

namespace Onyx.Oms.Web.Features.Orders.ShipOrder
{
    public class ShipOrderValidator : AbstractValidator<ShipOrderCommand>
    {
        public ShipOrderValidator()
        {
            RuleFor(x => x.OrderId).NotEmpty();
            RuleFor(x => x.CourierId).NotEmpty();
        }
    }
}
