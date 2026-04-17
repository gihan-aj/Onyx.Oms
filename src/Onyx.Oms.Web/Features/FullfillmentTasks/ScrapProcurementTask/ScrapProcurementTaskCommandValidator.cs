using FluentValidation;

namespace Onyx.Oms.Web.Features.FullfillmentTasks.ScrapProcurementTask;

public class ScrapProcurementTaskCommandValidator : AbstractValidator<ScrapProcurementTaskCommand>
{
    public ScrapProcurementTaskCommandValidator()
    {
        RuleFor(x => x.QuantityToScrap).GreaterThan(0).WithMessage("Quantity to scrap must be greater than zero.");
    }
}
