using FluentValidation;

namespace Onyx.Oms.Web.Features.Settings.TenantProfile.UpdateRegionalSettings;

public class UpdateRegionalSettingsValidator : AbstractValidator<UpdateRegionalSettingsCommand>
{
    public UpdateRegionalSettingsValidator()
    {
        RuleFor(x => x.DefaultCurrency)
            .NotEmpty().WithMessage("Base currency is required.")
            .Length(3).WithMessage("Base currency must be exactly 3 characters (e.g., LKR, USD).");

        RuleFor(x => x.TimeZone)
            .NotEmpty().WithMessage("Time zone is required.")
            .MaximumLength(50).WithMessage("Time zone cannot exceed 100 characters.");

        RuleFor(x => x.WeightUnit)
            .NotEmpty().WithMessage("Weight unit is required.")
            .MaximumLength(10).WithMessage("Weight unit cannot exceed 10 characters.");
    }
}
