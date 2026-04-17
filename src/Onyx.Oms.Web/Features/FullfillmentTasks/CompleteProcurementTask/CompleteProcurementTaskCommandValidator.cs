using FluentValidation;

namespace Onyx.Oms.Web.Features.FullfillmentTasks.CompleteProcurementTask;

public class CompleteProcurementTaskCommandValidator : AbstractValidator<CompleteProcurementTaskCommand>
{
    public CompleteProcurementTaskCommandValidator()
    {
        RuleFor(x => x.QuantityToComplete).GreaterThan(0).WithMessage("Quantity to complete must be greater than zero.");
    }
}
