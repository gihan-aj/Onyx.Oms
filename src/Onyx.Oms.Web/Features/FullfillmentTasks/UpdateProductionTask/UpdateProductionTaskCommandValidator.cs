using FluentValidation;

namespace Onyx.Oms.Web.Features.FullfillmentTasks.UpdateProductionTask;

public class UpdateProductionTaskCommandValidator : AbstractValidator<UpdateProductionTaskCommand>
{
    public UpdateProductionTaskCommandValidator()
    {
        RuleFor(x => x.RequestedQuantity).GreaterThan(0).WithMessage("Requested quantity must be greater than zero.");
        RuleFor(x => x.Priority).IsInEnum().WithMessage("Invalid priority specified.");
    }
}
