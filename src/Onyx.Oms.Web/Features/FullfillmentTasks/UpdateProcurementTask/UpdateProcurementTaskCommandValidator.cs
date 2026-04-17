using FluentValidation;

namespace Onyx.Oms.Web.Features.FullfillmentTasks.UpdateProcurementTask;

public class UpdateProcurementTaskCommandValidator : AbstractValidator<UpdateProcurementTaskCommand>
{
    public UpdateProcurementTaskCommandValidator()
    {
        RuleFor(x => x.RequestedQuantity).GreaterThan(0).WithMessage("Requested quantity must be greater than zero.");
        RuleFor(x => x.Priority).IsInEnum().WithMessage("Invalid priority specified.");
        RuleFor(x => x.Cost)
                .NotNull().WithMessage("Cost is required.")
                .SetValidator(new MoneyDtoValidator());

        RuleFor(x => x.PurchaseOrderNumber)
            .NotEmpty().WithMessage("Purchase order number is required.")
            .MaximumLength(100).WithMessage("Purchase order number must not exceed 100 characters.");
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
