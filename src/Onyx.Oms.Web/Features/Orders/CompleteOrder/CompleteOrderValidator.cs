using FluentValidation;

namespace Onyx.Oms.Web.Features.Orders.CompleteOrder
{
    public class CompleteOrderValidator : AbstractValidator<CompleteOrderCommand>
    {
        public CompleteOrderValidator()
        {
            RuleFor(x => x.OrderId).NotEmpty();
        }
    }
}
