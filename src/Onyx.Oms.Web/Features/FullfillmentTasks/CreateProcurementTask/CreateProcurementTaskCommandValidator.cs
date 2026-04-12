using FluentValidation;

namespace Onyx.Oms.Web.Features.FullfillmentTasks.CreateProcurementTask
{
    public class CreateProcurementTaskCommandValidator : AbstractValidator<CreateProcurementTaskCommand>
    {
        public CreateProcurementTaskCommandValidator()
        {
            RuleFor(x => x.ProductVariantId)
                .NotEmpty().WithMessage("Product variant ID is required.");
            RuleFor(x => x.RequestedQuantity)
                .GreaterThan(0).WithMessage("Requested quantity must be greater than zero.");
            RuleFor(x => x.Cost)
                .NotNull().WithMessage("Cost is required.")
                .SetValidator(new MoneyDtoValidator());
            RuleFor(x => x.PurchaseOrderNumber)
                .NotEmpty().WithMessage("Purchase order number is required.")
                .MaximumLength(100).WithMessage("Purchase order number must not exceed 100 characters.");
            RuleFor(x => x.Notes)
                .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters.")
                .When(x => !string.IsNullOrEmpty(x.Notes));
            RuleFor(x => x.ExpectedCompletionDate)
                .GreaterThan(DateTimeOffset.UtcNow).WithMessage("Expected completion date must be in the future.")
                .When(x => x.ExpectedCompletionDate.HasValue);
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
