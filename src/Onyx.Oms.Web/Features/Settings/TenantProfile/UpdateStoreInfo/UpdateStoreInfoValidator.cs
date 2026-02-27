using FluentValidation;

namespace Onyx.Oms.Web.Features.Settings.TenantProfile.UpdateStoreInfo;

public class UpdateStoreInfoValidator : AbstractValidator<UpdateStoreInfoCommand>
{
    public UpdateStoreInfoValidator()
    {
        RuleFor(x => x.StoreName)
            .NotEmpty().WithMessage("Store name is required.")
            .MaximumLength(200).WithMessage("Store name cannot exceed 200 characters.");

        RuleFor(x => x.ContactEmail)
            .NotEmpty().WithMessage("Contact email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(255).WithMessage("Email cannot exceed 255 characters.");

        RuleFor(x => x.LegalName).MaximumLength(200);
        RuleFor(x => x.TaxRegistrationNumber).MaximumLength(100);
        RuleFor(x => x.ContactPhone).MaximumLength(50);
    }
}
