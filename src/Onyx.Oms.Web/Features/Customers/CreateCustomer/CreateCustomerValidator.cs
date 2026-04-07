using FluentValidation;

namespace Onyx.Oms.Web.Features.Customers.CreateCustomer;

public class CreateCustomerValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200);

        RuleFor(c => c.Email)
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(200)
            .When(c => !string.IsNullOrEmpty(c.Email));

        RuleFor(c => c.PrimaryPhone)
            .NotEmpty().WithMessage("Primary Phone is required.")
            .MaximumLength(50);
            
        RuleFor(c => c.SecondaryPhone)
            .MaximumLength(50);

        RuleFor(c => c.Notes)
            .MaximumLength(1000);
            
        // Address validation (if provided)
        RuleFor(c => c.Street).MaximumLength(200);
        RuleFor(c => c.City).MaximumLength(100);
        RuleFor(c => c.District).MaximumLength(100);
        RuleFor(c => c.State).MaximumLength(100);
        RuleFor(c => c.PostalCode).MaximumLength(20);
        RuleFor(c => c.Country).MaximumLength(100);
    }
}
