using FluentValidation;

namespace Onyx.Oms.Web.Features.FullfillmentTasks.ScrapProductionTask;

public class ScrapProductionTaskCommandValidator : AbstractValidator<ScrapProductionTaskCommand>
{
    public ScrapProductionTaskCommandValidator()
    {
        RuleFor(x => x.QuantityToScrap).GreaterThan(0).WithMessage("Quantity to scrap must be greater than zero.");
    }
}
