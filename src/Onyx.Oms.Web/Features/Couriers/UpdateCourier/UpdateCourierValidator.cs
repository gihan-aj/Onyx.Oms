using FluentValidation;

namespace Onyx.Oms.Web.Features.Couriers.UpdateCourier;

public class UpdateCourierValidator : AbstractValidator<UpdateCourierCommand>
{
    public UpdateCourierValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty();

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

        When(c => c.ZoneRates != null && c.ZoneRates.Any(), () =>
        {
            RuleFor(c => c.ZoneRates)
                .Must(zones => zones!.Count(z => z.IsDefault) <= 1)
                .WithMessage("Only one zone rate can be marked as the default.");

            RuleForEach(c => c.ZoneRates).SetValidator(new UpdateCourierZoneRateDtoValidator());
        });
    }
}

public class UpdateCourierZoneRateDtoValidator : AbstractValidator<UpdateCourierZoneRateDto>
{
    public UpdateCourierZoneRateDtoValidator()
    {
        RuleFor(z => z.ZoneName)
            .NotEmpty().WithMessage("Zone name is required.")
            .MaximumLength(100);

        RuleFor(z => z.BaseFee)
            .GreaterThanOrEqualTo(0).WithMessage("Base fee cannot be negative.");

        RuleFor(z => z.BaseWeight)
            .GreaterThanOrEqualTo(0).WithMessage("Base weight cannot be negative.");

        RuleFor(z => z.ExcessFeePerWeightUnit)
            .GreaterThanOrEqualTo(0).WithMessage("Excess fee per weight unit cannot be negative.");

        RuleFor(z => z.CodPercentage)
            .InclusiveBetween(0, 100).WithMessage("COD percentage must be between 0 and 100.");

        RuleFor(z => z.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .MaximumLength(10);

        RuleFor(z => z.WeightUnit)
            .NotEmpty().WithMessage("Weight unit is required.")
            .MaximumLength(10);
    }
}
