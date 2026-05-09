using FluentValidation;

namespace Onyx.Oms.Web.Features.Orders.AllocateOrderItemQuantity
{
    public class AllocateOrderItemQuantityCommandValidator : AbstractValidator<AllocateOrderItemQuantityCommand>
    {
        public AllocateOrderItemQuantityCommandValidator()
        {
            RuleFor(x => x.OrderId).NotEmpty().WithMessage("Order ID is required.");
            RuleFor(x => x.OrderItemId).NotEmpty().WithMessage("Order Item ID is required.");
            RuleFor(x => x.QuantityToAllocate)
                .GreaterThan(0).WithMessage("Quantity to allocate must be greater than zero.");
        }
    }
}
