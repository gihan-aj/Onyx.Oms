using FluentValidation;

namespace Onyx.Oms.Web.Features.FullfillmentTasks.StartProduction
{
    public class StartProductionCommandValidator : AbstractValidator<StartProductionCommand>
    {
        public StartProductionCommandValidator()
        {
            RuleFor(x => x.ProductionsTaskId).NotEmpty();

            RuleFor(x => x.QuantityToStart).GreaterThan(0);
        }
    }
}
