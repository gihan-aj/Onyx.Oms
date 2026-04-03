using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Settings.TenantProfile.UpdateHeroImage
{
    public record UpdateTenantHeroImageCommand(string HeroImageUrl) : ICommand;
}
