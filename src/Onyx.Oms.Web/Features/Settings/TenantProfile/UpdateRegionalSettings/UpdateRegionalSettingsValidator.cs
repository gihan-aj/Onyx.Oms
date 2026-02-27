using FluentValidation;

namespace Onyx.Oms.Web.Features.Settings.TenantProfile.UpdateRegionalSettings;

public class UpdateRegionalSettingsValidator : AbstractValidator<UpdateRegionalSettingsCommand>
{
    public UpdateRegionalSettingsValidator()
    {
        RuleFor(x => x.BaseCurrency)
            .NotEmpty().WithMessage("Base currency is required.")
            .Length(3).WithMessage("Base currency must be exactly 3 characters (e.g., LKR, USD).");

        RuleFor(x => x.WeightUnit)
            .NotEmpty().WithMessage("Weight unit is required.")
            .MaximumLength(10).WithMessage("Weight unit cannot exceed 10 characters.");
    }
}
