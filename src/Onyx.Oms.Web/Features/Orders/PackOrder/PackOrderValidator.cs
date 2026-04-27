using FluentValidation;

namespace Onyx.Oms.Web.Features.Orders.PackOrder
{
    public class PackOrderValidator : AbstractValidator<PackOrderCommand>
    {
        public PackOrderValidator()
        {
            RuleFor(x => x.OrderId).NotEmpty();
        }
    }
}
