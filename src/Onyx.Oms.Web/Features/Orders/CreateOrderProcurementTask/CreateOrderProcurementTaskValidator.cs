using FluentValidation;

namespace Onyx.Oms.Web.Features.Orders.CreateOrderProcurementTask
{
    public class CreateOrderProcurementTaskValidator : AbstractValidator<CreateOrderProcurementTaskCommand>
    {
        public CreateOrderProcurementTaskValidator()
        {
            RuleFor(x => x.OrderId).NotEmpty();
            RuleFor(x => x.OrderItemId).NotEmpty();
            RuleFor(x => x.RequestedQuantity).GreaterThan(0);
        }
    }
}
