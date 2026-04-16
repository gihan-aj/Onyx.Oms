using FluentValidation;

namespace Onyx.Oms.Web.Features.FullfillmentTasks.IssuePurchaseOrder
{
    public class IssuePurchaseOrderCommandValidator : AbstractValidator<IssuePurchaseOrderCommand>
    {
        public IssuePurchaseOrderCommandValidator()
        {
            RuleFor(x => x.ProcurementTaskId).NotEmpty();

            RuleFor(x => x.IssueQuantity).GreaterThan(0);

            RuleFor(x => x.Cost)
                .NotNull().WithMessage("Cost is required.")
                .SetValidator(new MoneyDtoValidator());

            RuleFor(x => x.PurchaseOrderNumber)
                .NotEmpty().WithMessage("Purchase order number is required.")
                .MaximumLength(100).WithMessage("Purchase order number must not exceed 100 characters.");
        }
    }

    public class MoneyDtoValidator : AbstractValidator<MoneyDto>
    {
        public MoneyDtoValidator()
        {
            RuleFor(x => x.Amount)
                .GreaterThanOrEqualTo(0).WithMessage("Amount cannot be negative.");

            RuleFor(x => x.Currency)
                .NotEmpty().WithMessage("Currency is required.")
                .Length(3).WithMessage("Currency must be a 3-letter code (e.g., 'LKR').");
        }
    }
}
