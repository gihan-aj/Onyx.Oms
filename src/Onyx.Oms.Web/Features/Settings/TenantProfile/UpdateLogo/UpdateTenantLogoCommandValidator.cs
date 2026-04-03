using FluentValidation;

namespace Onyx.Oms.Web.Features.Settings.TenantProfile.UpdateLogo
{
    public class UpdateTenantLogoCommandValidator : AbstractValidator<UpdateTenantLogoCommand>
    {
        public UpdateTenantLogoCommandValidator()
        {
            RuleFor(x => x.LogoUrl)
                .NotEmpty().WithMessage("Logo URL is required.")
                .MaximumLength(500).WithMessage("Logo URL cannot exceed 500 characters.");
        }
    }
}
