using FluentValidation;

namespace Onyx.Oms.Web.Features.Orders.FailDelivery
{
    public class FailDeliveryValidator : AbstractValidator<FailDeliveryCommand>
    {
        public FailDeliveryValidator()
        {
            RuleFor(x => x.OrderId).NotEmpty();
        }
    }
}
