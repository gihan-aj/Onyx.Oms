using FluentValidation;

namespace Onyx.Oms.Web.Features.Orders.RollbackOrderToPacked
{
    public class RollbackOrderToPackedCommandValidator : AbstractValidator<RollbackOrderToPackedCommand>
    {
        public RollbackOrderToPackedCommandValidator()
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
