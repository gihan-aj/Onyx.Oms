using FluentValidation;

namespace Onyx.Oms.Web.Features.Couriers.CreateCourier;

public class CreateCourierValidator : AbstractValidator<CreateCourierCommand>
{
    public CreateCourierValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200);

        RuleFor(c => c.ContactPerson)
            .MaximumLength(200);

        RuleFor(c => c.PrimaryPhone)
            .MaximumLength(50);
            
        RuleFor(c => c.SecondaryPhone)
            .MaximumLength(50);

        RuleFor(c => c.WebsiteUrl)
            .MaximumLength(500);
            
        RuleFor(c => c.TrackingUrlTemplate)
            .MaximumLength(500);
    }
}
