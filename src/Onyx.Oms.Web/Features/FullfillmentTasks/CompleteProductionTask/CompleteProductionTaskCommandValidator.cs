using FluentValidation;

namespace Onyx.Oms.Web.Features.FullfillmentTasks.CompleteProductionTask;

public class CompleteProductionTaskCommandValidator : AbstractValidator<CompleteProductionTaskCommand>
{
    public CompleteProductionTaskCommandValidator()
    {
        RuleFor(x => x.QuantityToComplete).GreaterThan(0).WithMessage("Quantity to complete must be greater than zero.");
    }
}
