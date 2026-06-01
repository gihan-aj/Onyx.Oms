using FluentValidation;

namespace Onyx.Oms.Web.Features.Expenses.CreateExpense
{
    public class CreateExpenseCommandValidator : AbstractValidator<CreateExpenseCommand>
    {
        public CreateExpenseCommandValidator()
        {
            RuleFor(x => x.Category)
                .NotEmpty().WithMessage("Expense category is required.")
                .MaximumLength(200).WithMessage("Expense category must not exceed 200 characters.");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Expense amount must be greater than zero.");

            RuleFor(x => x.Currency)
                .NotEmpty().WithMessage("Currency is required.")
                .Length(3).WithMessage("Currency must be a 3-letter ISO code.");

            RuleFor(x => x.Reference)
                .MaximumLength(500).WithMessage("Reference must not exceed 500 characters.");

            RuleFor(x => x.Notes)
                .MaximumLength(500).WithMessage("Notes must not exceed 500 characters.");
        }
    }
}
