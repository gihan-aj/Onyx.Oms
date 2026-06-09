using FluentValidation;

namespace Onyx.Oms.Web.Features.Orders.RollbackOrderToPending
{
    public class RollbackOrderToPendingCommandValidator : AbstractValidator<RollbackOrderToPendingCommand>
    {
        public RollbackOrderToPendingCommandValidator()
        {
            RuleFor(x => x.OrderId)
                .NotEmpty().WithMessage("Order ID is required.");

            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Rollback reason is required.")
                .MinimumLength(10).WithMessage("Rollback reason must be at least 10 characters long.")
                .MaximumLength(500).WithMessage("Rollback reason cannot exceed 500 characters.");
        }
    }
}
