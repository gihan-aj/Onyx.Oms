using FluentValidation;

namespace Onyx.Oms.Web.Features.Settings.WhatsApp.UpdateWhatsAppSettings
{
    public class UpdateWhatsAppSettingsCommandValidator : AbstractValidator<UpdateWhatsAppSettingsCommand>
    {
        public UpdateWhatsAppSettingsCommandValidator()
        {
            RuleFor(x => x.PhoneNumberId)
                .NotEmpty().WithMessage("Phone Number ID is required.")
                .MaximumLength(50).WithMessage("Phone Number ID cannot exceed 500 characters.");

            RuleFor(x => x.AccessToken)
                .MaximumLength(4000).WithMessage("Access Token cannot exceed 4000 characters.")
                .When(x => !string.IsNullOrEmpty(x.AccessToken));
        }
    }
}
