using FluentValidation;

namespace Onyx.Oms.Web.Features.Orders.CreateOrderProductionTask
{
    public class CreateOrderProductionTaskValidator : AbstractValidator<CreateOrderProductionTaskCommand>
    {
        public CreateOrderProductionTaskValidator()
        {
            RuleFor(x => x.OrderId).NotEmpty();
            RuleFor(x => x.OrderItemId).NotEmpty();
            RuleFor(x => x.RequestedQuantity).GreaterThan(0);
        }
    }
}
