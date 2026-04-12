using FluentValidation;

namespace Onyx.Oms.Web.Features.FullfillmentTasks.CreateProductionTask
{
    public class CreateProductionTaskCommandValidator : AbstractValidator<CreateProductionTaskCommand>
    {
        public CreateProductionTaskCommandValidator()
        {
            RuleFor(x => x.ProductVariantId)
                .NotEmpty().WithMessage("Product variant ID is required.");

            RuleFor(x => x.RequestedQuantity)
                .GreaterThan(0).WithMessage("Requested quantity must be greater than zero.");

            RuleFor(x => x.Notes)
                .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters.")
                .When(x => !string.IsNullOrEmpty(x.Notes));

            RuleFor(x => x.ExpectedCompletionDate)
                .GreaterThan(DateTimeOffset.UtcNow).WithMessage("Expected completion date must be in the future.")
                .When(x => x.ExpectedCompletionDate.HasValue);
        }
    }
}
