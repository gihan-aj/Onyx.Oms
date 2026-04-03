using FluentValidation;

namespace Onyx.Oms.Web.Features.Settings.TenantProfile.UpdateHeroImage
{
    public class UpdateTenantHeroImageCommandValidator : AbstractValidator<UpdateTenantHeroImageCommand>
    {
        public UpdateTenantHeroImageCommandValidator()
        {
            RuleFor(x => x.HeroImageUrl)
                .NotEmpty().WithMessage("Hero Image URL is required.")
                .MaximumLength(500).WithMessage("Hero Image URL cannot exceed 500 characters.");
        }
    }
}
